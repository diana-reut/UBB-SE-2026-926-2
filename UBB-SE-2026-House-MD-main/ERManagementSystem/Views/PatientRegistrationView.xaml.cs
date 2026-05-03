using System.ComponentModel;
using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class PatientRegistrationView : Page
    {
        public PatientRegistrationViewModel? ViewModel { get; private set; }

        public PatientRegistrationView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is PatientRegistrationViewModel vm)
            {
                ViewModel = vm;

                // Listen for property changes so we can reset
                // controls that are not directly x:Bind-able
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                Bindings.Update();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Unsubscribe to avoid memory leaks
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When HasDateOfBirth is reset to false by ClearForm(),
            // clear the CalendarDatePicker in the UI
            if (e.PropertyName == nameof(ViewModel.HasDateOfBirth))
            {
                if (ViewModel?.HasDateOfBirth == false)
                {
                    DateOfBirthPicker.Date = null;
                }
            }

            // When Gender is reset to empty by ClearForm(),
            // clear the ComboBox selection in the UI
            if (e.PropertyName == nameof(ViewModel.Gender))
            {
                if (string.IsNullOrEmpty(ViewModel?.Gender))
                {
                    GenderComboBox.SelectedItem = null;
                }
            }
        }

        private void DateOfBirthPicker_DateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (args.NewDate.HasValue)
            {
                ViewModel.DateOfBirth = args.NewDate.Value;
                ViewModel.HasDateOfBirth = true;
            }
            else
            {
                ViewModel.HasDateOfBirth = false;
            }
        }

        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.Gender = item.Tag?.ToString() ?? string.Empty;
            }
        }
    }
}