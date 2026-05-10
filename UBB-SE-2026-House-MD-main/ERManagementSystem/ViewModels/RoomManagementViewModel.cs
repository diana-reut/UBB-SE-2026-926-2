using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Proxy.ERRoomProxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.ExaminationProxy;
using ERManagementSystem.Proxy.TriageProxy;
using ERManagementSystem.Repositories;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class RoomManagementViewModel : BaseViewModel
    {
        private readonly IERRoomProxy erRoomProxy;
        private readonly IERVisitProxy erVisitProxy;
        private readonly IExaminationProxy examinationProxy;
        private readonly ITriageProxy triageProxy;
        private readonly IPatientRepository patientRepository;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomManagementViewModel(
            IERRoomProxy erRoomProxy,
            IERVisitProxy erVisitProxy,
            IExaminationProxy examinationProxy,
            ITriageProxy triageProxy,
            IPatientRepository patientRepository)
        {
            this.erRoomProxy = erRoomProxy;
            this.erVisitProxy = erVisitProxy;
            this.examinationProxy = examinationProxy;
            this.triageProxy = triageProxy;
            this.patientRepository = patientRepository;
        }

        [ObservableProperty] private Patient? selectedPatient;
        [ObservableProperty] private ER_Visit? selectedVisit;
        [ObservableProperty] private Triage? selectedTriage;

        partial void OnSelectedOccupiedRoomChanged(ER_Room? value)
            => _ = HandleSelectedOccupiedRoomChangedAsync(value);

        partial void OnSelectedCleaningRoomChanged(ER_Room? value)
            => _ = HandleSelectedCleaningRoomChangedAsync(value);

        private async Task HandleSelectedOccupiedRoomChangedAsync(ER_Room? value)
        {
            if (value != null)
            {
                await LoadRoomVisit(value);
            }
            else if (SelectedCleaningRoom == null)
            {
                ClearVisitDetails();
            }
        }

        private async Task HandleSelectedCleaningRoomChangedAsync(ER_Room? value)
        {
            if (value != null)
            {
                await LoadRoomVisit(value);
            }
            else if (SelectedOccupiedRoom == null)
            {
                ClearVisitDetails();
            }
        }

        private async Task LoadRoomVisit(ER_Room room)
        {
            try
            {
                var roomVisitDetails = await GetRoomVisitDetailsAsync(room);
                if (roomVisitDetails == null)
                {
                    ClearVisitDetails();
                    return;
                }

                SelectedVisit = roomVisitDetails.Visit;
                SelectedPatient = roomVisitDetails.Patient;
                SelectedTriage = roomVisitDetails.Triage;
            }
            catch
            {
                ClearVisitDetails();
            }
        }

        private void ClearVisitDetails()
        {
            SelectedPatient = null;
            SelectedVisit = null;
            SelectedTriage = null;
        }

        private async Task<RoomVisitDetails?> GetRoomVisitDetailsAsync(ER_Room room)
        {
            ER_Visit? visit = null;

            if (room.Current_Visit_ID.HasValue)
            {
                visit = await erVisitProxy.GetByIdAsync(room.Current_Visit_ID.Value);
            }

            if (visit == null)
            {
                var examinations = await examinationProxy.GetAllAsync();
                int? fallbackVisitId = examinations
                    .Where(examination => examination.Room_ID == room.Room_ID)
                    .OrderByDescending(examination => examination.Exam_Time)
                    .Select(examination => (int?)examination.Visit_ID)
                    .FirstOrDefault();

                if (fallbackVisitId.HasValue)
                {
                    visit = await erVisitProxy.GetByIdAsync(fallbackVisitId.Value);
                }
            }

            if (visit == null)
            {
                return null;
            }

            return new RoomVisitDetails
            {
                Visit = visit,
                Patient = await patientRepository.GetByIdAsync(visit.Patient_ID),
                Triage = await triageProxy.GetByVisitIdAsync(visit.Visit_ID)
            };
        }

        [ObservableProperty] private ObservableCollection<ER_Room> availableRooms = new ObservableCollection<ER_Room>();
        [ObservableProperty] private ObservableCollection<ER_Room> occupiedRooms = new ObservableCollection<ER_Room>();
        [ObservableProperty] private ObservableCollection<ER_Room> cleaningRooms = new ObservableCollection<ER_Room>();

        [ObservableProperty] private int totalRooms;
        [ObservableProperty] private int availableCount;
        [ObservableProperty] private int occupiedCount;
        [ObservableProperty] private int cleaningCount;

        [ObservableProperty] private ER_Room? selectedOccupiedRoom;
        [ObservableProperty] private ER_Room? selectedCleaningRoom;
        [ObservableProperty] private string statusMessage = string.Empty;

        [RelayCommand]
        public async Task LoadRooms()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                AvailableRooms = new ObservableCollection<ER_Room>(await erRoomProxy.GetAvailableRoomsAsync());
                OccupiedRooms = new ObservableCollection<ER_Room>(await erRoomProxy.GetOccupiedRoomsAsync());
                CleaningRooms = new ObservableCollection<ER_Room>(await erRoomProxy.GetCleaningRoomsAsync());

                AvailableCount = AvailableRooms.Count;
                OccupiedCount = OccupiedRooms.Count;
                CleaningCount = CleaningRooms.Count;
                TotalRooms = AvailableCount + OccupiedCount + CleaningCount;
            }
            catch (Exception ex)
            {
                Logger.Error("RoomManagementViewModel.LoadRooms failed.", ex);
                StatusMessage = $"Error loading rooms: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkRoomAsCleaning()
        {
            if (SelectedOccupiedRoom == null)
            {
                await ShowDialog("No Room Selected", "Please select an occupied room first.");
                return;
            }
            try
            {
                IsBusy = true;
                SelectedOccupiedRoom.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
                await erRoomProxy.UpdateAsync(SelectedOccupiedRoom.Room_ID, SelectedOccupiedRoom);
                await erRoomProxy.ClearCurrentVisitAsync(SelectedOccupiedRoom.Room_ID);
                await ShowDialog("Room Cleaning", $"Room {SelectedOccupiedRoom.Room_ID} ({SelectedOccupiedRoom.Room_Type}) is now being cleaned.");
                SelectedOccupiedRoom = null;
                await LoadRooms();
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkRoomAsCleaned()
        {
            if (SelectedCleaningRoom == null)
            {
                await ShowDialog("No Room Selected", "Please select a room in the Cleaning tab first.");
                return;
            }
            try
            {
                IsBusy = true;
                SelectedCleaningRoom.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);
                await erRoomProxy.UpdateAsync(SelectedCleaningRoom.Room_ID, SelectedCleaningRoom);
                await ShowDialog("Room Ready", $"Room {SelectedCleaningRoom.Room_ID} ({SelectedCleaningRoom.Room_Type}) is now available.");
                SelectedCleaningRoom = null;
                await LoadRooms();
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", ex.Message);
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
