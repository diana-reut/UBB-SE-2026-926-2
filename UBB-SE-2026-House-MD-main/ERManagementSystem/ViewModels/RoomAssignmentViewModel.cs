using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Proxy.ERRoomProxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.PatientProxy;
using ERManagementSystem.Proxy.TriageProxy;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class RoomAssignmentViewModel : BaseViewModel
    {
        private readonly IERVisitProxy erVisitProxy;
        private readonly IERRoomProxy erRoomProxy;
        private readonly ITriageProxy triageProxy;
        private readonly IPatientProxy patientProxy;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomAssignmentViewModel(
            IERVisitProxy erVisitProxy,
            IERRoomProxy erRoomProxy,
            ITriageProxy triageProxy,
            IPatientProxy patientProxy)
        {
            this.erVisitProxy = erVisitProxy;
            this.erRoomProxy = erRoomProxy;
            this.triageProxy = triageProxy;
            this.patientProxy = patientProxy;
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
                SelectedPatient = await patientProxy.GetByCnpAsync(value.Patient_ID);
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

                List<ER_Visit> waitingVisits = await erVisitProxy.GetByStatusAsync(ER_Visit.VisitStatus.WAITING_FOR_ROOM);
                List<Triage> triages = await triageProxy.GetAllAsync();
                IReadOnlyList<(ER_Visit visit, Triage triage)> waitingWithTriage = waitingVisits
                    .Join(
                        triages,
                        visit => visit.Visit_ID,
                        triage => triage.Visit_ID,
                        (visit, triage) => (visit, triage))
                    .OrderBy(queueEntry => queueEntry.triage.Triage_Level)
                    .ThenBy(queueEntry => queueEntry.visit.Arrival_date_time)
                    .ToList();
                WaitingVisits = new ObservableCollection<ER_Visit>();
                foreach (var (visit, _) in waitingWithTriage)
                {
                    WaitingVisits.Add(visit);
                }

                AvailableRooms = new ObservableCollection<ER_Room>(await erRoomProxy.GetAvailableRoomsAsync());
            }
            catch (Exception ex)
            {
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
                bool assigned = await erVisitProxy.AutoAssignHighestPriorityRoomAsync();
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
                await erVisitProxy.AssignRoomAsync(SelectedVisit.Visit_ID, SelectedRoom.Room_ID);
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
