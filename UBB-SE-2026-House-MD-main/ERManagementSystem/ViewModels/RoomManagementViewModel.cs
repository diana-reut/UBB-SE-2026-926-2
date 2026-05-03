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
    public partial class RoomManagementViewModel : BaseViewModel
    {
        private readonly IRoomManagementService roomManagementService;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomManagementViewModel(
            IRoomManagementService roomManagementService)
        {
            this.roomManagementService = roomManagementService;
        }

        [ObservableProperty] private Patient? selectedPatient;
        [ObservableProperty] private ER_Visit? selectedVisit;
        [ObservableProperty] private Triage? selectedTriage;

        partial void OnSelectedOccupiedRoomChanged(ER_Room? value)
        {
            if (value != null) LoadRoomVisit(value);
            else if (SelectedCleaningRoom == null) ClearVisitDetails();
        }

        partial void OnSelectedCleaningRoomChanged(ER_Room? value)
        {
            if (value != null) LoadRoomVisit(value);
            else if (SelectedOccupiedRoom == null) ClearVisitDetails();
        }

        private void LoadRoomVisit(ER_Room room)
        {
            try
            {
                var roomVisitDetails = roomManagementService.GetRoomVisitDetails(room.Room_ID);
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
        public void LoadRooms()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                AvailableRooms = new ObservableCollection<ER_Room>(roomManagementService.GetAvailableRooms());
                OccupiedRooms = new ObservableCollection<ER_Room>(roomManagementService.GetOccupiedRooms());
                CleaningRooms = new ObservableCollection<ER_Room>(roomManagementService.GetCleaningRooms());

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
                roomManagementService.MarkRoomAsCleaning(SelectedOccupiedRoom.Room_ID);
                await ShowDialog("Room Cleaning", $"Room {SelectedOccupiedRoom.Room_ID} ({SelectedOccupiedRoom.Room_Type}) is now being cleaned.");
                SelectedOccupiedRoom = null;
                LoadRooms();
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
                roomManagementService.MarkRoomAsCleaned(SelectedCleaningRoom.Room_ID);
                await ShowDialog("Room Ready", $"Room {SelectedCleaningRoom.Room_ID} ({SelectedCleaningRoom.Room_Type}) is now available.");
                SelectedCleaningRoom = null;
                LoadRooms();
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
