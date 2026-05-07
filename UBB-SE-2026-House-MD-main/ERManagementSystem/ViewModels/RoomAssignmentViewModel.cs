using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class RoomAssignmentViewModel : BaseViewModel
    {
        private readonly IRoomAssignmentService roomAssignmentService;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomAssignmentViewModel(
            IRoomAssignmentService roomAssignmentService)
        {
            this.roomAssignmentService = roomAssignmentService;
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
                SelectedPatient = await roomAssignmentService.GetPatientByIdAsync(value.Patient_ID);
                SelectedTriage = await roomAssignmentService.GetTriageByVisitIdAsync(value.Visit_ID);
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

                var waitingWithTriage = await roomAssignmentService.GetWaitingVisitsWithTriageAsync();
                WaitingVisits = new ObservableCollection<ER_Visit>();
                foreach (var (visit, _) in waitingWithTriage)
                {
                    WaitingVisits.Add(visit);
                }

                AvailableRooms = new ObservableCollection<ER_Room>(await roomAssignmentService.GetAvailableRoomsAsync());
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
                bool assigned = await roomAssignmentService.AutoAssignRoomAsync();
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
                await roomAssignmentService.AssignRoomToVisitAsync(SelectedVisit.Visit_ID, SelectedRoom.Room_ID);
                await ShowDialog("Room Assigned", $"Visit {SelectedVisit.Visit_ID} → Room {SelectedRoom.Room_ID} ({SelectedRoom.Room_Type}).");
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
