using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Helpers;
using ERManagementSystem.Proxy.ERRoomProxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.TriageParametersProxy;
using ERManagementSystem.Proxy.TriageProxy;
using ERManagementSystem.Repositories;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class RoomAssignmentViewModel : BaseViewModel
    {
        private readonly IERRoomProxy erRoomProxy;
        private readonly IERVisitProxy erVisitProxy;
        private readonly ITriageProxy triageProxy;
        private readonly ITriageParametersProxy triageParametersProxy;
        private readonly IPatientRepository patientRepository;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomAssignmentViewModel(
            IERRoomProxy erRoomProxy,
            IERVisitProxy erVisitProxy,
            ITriageProxy triageProxy,
            ITriageParametersProxy triageParametersProxy,
            IPatientRepository patientRepository)
        {
            this.erRoomProxy = erRoomProxy;
            this.erVisitProxy = erVisitProxy;
            this.triageProxy = triageProxy;
            this.triageParametersProxy = triageParametersProxy;
            this.patientRepository = patientRepository;
        }

        [ObservableProperty] private ObservableCollection<ER_Visit> waitingVisits = new ObservableCollection<ER_Visit>();
        [ObservableProperty] private ObservableCollection<ER_Room> availableRooms = new ObservableCollection<ER_Room>();
        [ObservableProperty] private ER_Visit? selectedVisit;
        [ObservableProperty] private ER_Room? selectedRoom;
        [ObservableProperty] private Patient? selectedPatient;
        [ObservableProperty] private Triage? selectedTriage;
        [ObservableProperty] private string statusMessage = string.Empty;

        partial void OnSelectedVisitChanged(ER_Visit? value)
            => _ = HandleSelectedVisitChangedAsync(value);

        private async Task HandleSelectedVisitChangedAsync(ER_Visit? value)
        {
            if (value == null)
            {
                SelectedPatient = null;
                SelectedTriage = null;
                return;
            }

            try
            {
                SelectedPatient = await patientRepository.GetByIdAsync(value.Patient_ID);
                SelectedTriage = await triageProxy.GetByVisitIdAsync(value.Visit_ID);
            }
            catch
            {
                SelectedPatient = null;
                SelectedTriage = null;
            }
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                var waitingWithTriage = await GetWaitingVisitsWithTriageAsync();
                WaitingVisits = new ObservableCollection<ER_Visit>();
                foreach (var (visit, _) in waitingWithTriage)
                {
                    WaitingVisits.Add(visit);
                }

                AvailableRooms = new ObservableCollection<ER_Room>(await erRoomProxy.GetAvailableRoomsAsync());
            }
            catch (Exception ex)
            {
                Logger.Error("RoomAssignmentViewModel.LoadData failed.", ex);
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AssignRoom()
        {
            if (WaitingVisits.Count == 0)
            {
                await ShowDialog("No Waiting Visits", "There are no visits currently waiting for a room.");
                return;
            }
            try
            {
                IsBusy = true;
                bool assigned = await AutoAssignRoomAsync();
                if (assigned)
                {
                    await ShowDialog("Room Assigned", "The highest-priority visit has been automatically assigned to a matching room.");
                    await LoadData();
                }
                else
                {
                    await ShowDialog("No Suitable Room", "No proper room matching this patient's requirements is currently available.\n\nPlease either wait for the required room to open up or manually assign them to an available room.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Assignment Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ManualAssignRoom()
        {
            if (SelectedVisit == null || SelectedRoom == null)
            {
                await ShowDialog("Selection Required", "Please select both a waiting visit and an available room.");
                return;
            }
            if (!ER_Room.StatusEquals(SelectedRoom.Availability_Status, ER_Room.RoomStatus.Available))
            {
                await ShowDialog("Room Not Available", $"Room {SelectedRoom.Room_ID} is '{SelectedRoom.Availability_Status}'. Only available rooms can be assigned.");
                return;
            }
            if (SelectedVisit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                await ShowDialog("Visit Not Waiting", $"Visit {SelectedVisit.Visit_ID} is in '{SelectedVisit.Status}'. Only WAITING_FOR_ROOM visits can be assigned.");
                return;
            }
            try
            {
                IsBusy = true;
                await AssignRoomToVisitAsync(SelectedVisit.Visit_ID, SelectedRoom.Room_ID);
                await ShowDialog("Room Assigned", $"Visit {SelectedVisit.Visit_ID} -> Room {SelectedRoom.Room_ID} ({SelectedRoom.Room_Type}).");
                SelectedVisit = null;
                SelectedRoom = null;
                await LoadData();
            }
            catch (Exception ex)
            {
                await ShowDialog("Assignment Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<IReadOnlyList<(ER_Visit visit, Triage triage)>> GetWaitingVisitsWithTriageAsync()
        {
            var waitingVisitsWithStatus = await erVisitProxy.GetByStatusAsync(ER_Visit.VisitStatus.WAITING_FOR_ROOM);
            var triages = await triageProxy.GetAllAsync();

            return waitingVisitsWithStatus
                .Join(
                    triages,
                    visit => visit.Visit_ID,
                    triage => triage.Visit_ID,
                    (visit, triage) => (visit, triage))
                .OrderBy(queueEntry => queueEntry.triage.Triage_Level)
                .ThenBy(queueEntry => queueEntry.visit.Arrival_date_time)
                .ToList();
        }

        private async Task<bool> AutoAssignRoomAsync()
        {
            IReadOnlyList<(ER_Visit visit, Triage triage)> waitingWithTriage = await GetWaitingVisitsWithTriageAsync();
            if (waitingWithTriage.Count == 0)
            {
                return false;
            }

            var (topVisit, topTriage) = waitingWithTriage.First();
            Triage_Parameters? parameters = await triageParametersProxy.GetByTriageIdAsync(topTriage.Triage_ID);

            string requiredType = RoomTypeHelper.DetermineRoomType(
                topTriage.Specialization,
                parameters?.Bleeding ?? 1,
                parameters?.Injury_Type ?? 1,
                parameters?.Consciousness ?? 1,
                parameters?.Breathing ?? 1);

            ER_Room? room = (await erRoomProxy.GetAvailableRoomsAsync())
                .FirstOrDefault(availableRoom => availableRoom.Room_Type == requiredType);

            if (room == null)
            {
                Logger.Warning($"AutoAssignRoom: No '{requiredType}' room available for Visit {topVisit.Visit_ID}.");
                return false;
            }

            await AssignRoomToVisitAsync(topVisit.Visit_ID, room.Room_ID);
            return true;
        }

        private async Task AssignRoomToVisitAsync(int visitId, int roomId)
        {
            ER_Room room = await erRoomProxy.GetByIdAsync(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (!ER_Room.StatusEquals(room.Availability_Status, ER_Room.RoomStatus.Available))
            {
                throw new InvalidOperationException(
                    $"Room {roomId} is not available (current: '{room.Availability_Status}').");
            }

            ER_Visit visit = await erVisitProxy.GetByIdAsync(visitId)
                ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

            if (visit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                throw new InvalidOperationException(
                    $"Visit {visitId} is not in WAITING_FOR_ROOM (current: '{visit.Status}').");
            }

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Occupied);
            await erRoomProxy.UpdateAsync(roomId, room);
            await erRoomProxy.SetCurrentVisitAsync(roomId, visitId);
            await erVisitProxy.UpdateStatusAsync(visitId, ER_Visit.VisitStatus.IN_ROOM);
            Logger.Info($"Visit {visitId} assigned to Room {roomId}.");
        }

        private async Task ShowDialog(string title, string message)
        {
            if (XamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
