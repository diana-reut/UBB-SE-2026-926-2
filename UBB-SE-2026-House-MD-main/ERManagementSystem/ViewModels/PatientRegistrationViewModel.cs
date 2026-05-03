using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class PatientRegistrationViewModel : BaseViewModel
    {
        private readonly IRegistrationService registrationService;

        public PatientRegistrationViewModel(IRegistrationService registrationService)
        {
            this.registrationService = registrationService;
        }

        [ObservableProperty]
        private string patientId = string.Empty;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private DateTimeOffset dateOfBirth = DateTimeOffset.Now;

        [ObservableProperty]
        private bool hasDateOfBirth = false;

        [ObservableProperty]
        private string gender = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string emergencyContact = string.Empty;

        [ObservableProperty]
        private string chiefComplaint = string.Empty;

        [ObservableProperty]
        private string patientIdError = string.Empty;

        [ObservableProperty]
        private string firstNameError = string.Empty;

        [ObservableProperty]
        private string lastNameError = string.Empty;

        [ObservableProperty]
        private string dateOfBirthError = string.Empty;

        [ObservableProperty]
        private string phoneError = string.Empty;

        [ObservableProperty]
        private string emergencyContactError = string.Empty;

        [ObservableProperty]
        private string chiefComplaintError = string.Empty;
        private bool submitAttempted = false;

        partial void OnPatientIdChanged(string value) => ValidateAll();
        partial void OnFirstNameChanged(string value) => ValidateAll();
        partial void OnLastNameChanged(string value) => ValidateAll();
        partial void OnDateOfBirthChanged(DateTimeOffset value) => ValidateAll();
        partial void OnHasDateOfBirthChanged(bool value) => ValidateAll();
        partial void OnPhoneChanged(string value) => ValidateAll();
        partial void OnEmergencyContactChanged(string value) => ValidateAll();
        partial void OnChiefComplaintChanged(string value) => ValidateAll();
        private bool ValidateAll()
        {
            bool valid = true;

            // Patient ID — exactly 13 digits
            if (string.IsNullOrWhiteSpace(PatientId))
            {
                if (submitAttempted)
                {
                    PatientIdError = "Patient ID (CNP) is required.";
                }

                valid = false;
            }
            else if (!Regex.IsMatch(PatientId, @"^\d{13}$"))
            {
                if (submitAttempted)
                {
                    PatientIdError = "CNP must be exactly 13 digits.";
                }

                valid = false;
            }
            else
            {
                PatientIdError = string.Empty;
            }

            // First Name
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                if (submitAttempted)
                {
                    FirstNameError = "First name is required.";
                }

                valid = false;
            }
            else
            {
                FirstNameError = string.Empty;
            }

            // Last Name
            if (string.IsNullOrWhiteSpace(LastName))
            {
                if (submitAttempted)
                {
                    LastNameError = "Last name is required.";
                }

                valid = false;
            }
            else
            {
                LastNameError = string.Empty;
            }

            // Date of Birth
            if (!HasDateOfBirth)
            {
                if (submitAttempted)
                {
                    DateOfBirthError = "Date of birth is required.";
                }

                valid = false;
            }
            else if (DateOfBirth >= DateTimeOffset.Now)
            {
                if (submitAttempted)
                {
                    DateOfBirthError = "Date of birth must be in the past.";
                }

                valid = false;
            }
            else
            {
                DateOfBirthError = string.Empty;
            }

            // Phone — Romanian format 07XXXXXXXX
            if (string.IsNullOrWhiteSpace(Phone))
            {
                if (submitAttempted)
                {
                    PhoneError = "Phone number is required.";
                }

                valid = false;
            }
            else if (!Regex.IsMatch(Phone, @"^07\d{8}$"))
            {
                if (submitAttempted)
                {
                    PhoneError = "Phone must be in format 07XXXXXXXX.";
                }

                valid = false;
            }
            else
            {
                PhoneError = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(EmergencyContact))
            {
                if (submitAttempted)
                {
                    EmergencyContactError = "Emergency contact is required.";
                }

                valid = false;
            }
            else if (!Regex.IsMatch(EmergencyContact,
                @"^[A-Za-zÀ-ÖØ-öø-ÿ]+(?:[\s\-][A-Za-zÀ-ÖØ-öø-ÿ]+)+ - 07\d{8}$"))
            {
                if (submitAttempted)
                {
                    EmergencyContactError =
                    "Format: Firstname Lastname - 07XXXXXXXX";
                }

                valid = false;
            }
            else
            {
                EmergencyContactError = string.Empty;
            }

            // Chief Complaint
            if (string.IsNullOrWhiteSpace(ChiefComplaint))
            {
                if (submitAttempted)
                {
                    ChiefComplaintError = "Chief complaint is required.";
                }

                valid = false;
            }
            else
            {
                ChiefComplaintError = string.Empty;
            }

            return valid;
        }

        private Microsoft.UI.Xaml.XamlRoot? GetXamlRoot()
            => App.MainAppWindow?.Content?.XamlRoot;

        [RelayCommand]
        private async Task RegisterPatientAndVisit()
        {
            submitAttempted = true;

            if (!ValidateAll())
            {
                var validationDialog = new ContentDialog
                {
                    Title = "Invalid Data",
                    Content = "Some fields are missing or incorrect.\nPlease check the highlighted fields and try again.",
                    CloseButtonText = "OK",
                    XamlRoot = GetXamlRoot()
                };

                await validationDialog.ShowAsync();
                return;
            }

            try
            {
                var patient = new Patient
                {
                    Patient_ID = PatientId,
                    First_Name = FirstName,
                    Last_Name = LastName,
                    Date_of_Birth = DateOfBirth.DateTime,
                    Gender = Gender,
                    Phone = Phone,
                    Emergency_Contact = EmergencyContact
                };

                ER_Visit visit = registrationService.RegisterPatientAndVisit(patient, ChiefComplaint);

                var successDialog = new ContentDialog
                {
                    Title = "Registration Successful",
                    Content = $"Patient ID: {patient.Patient_ID}\n" +
                                      $"Visit ID:   {visit.Visit_ID}\n" +
                                      $"Status:     {visit.Status}",
                    CloseButtonText = "OK",
                    XamlRoot = GetXamlRoot()
                };

                await successDialog.ShowAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Registration Failed",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = GetXamlRoot()
                };

                await errorDialog.ShowAsync();
            }
        }

        [RelayCommand]
        public void ClearForm()
        {
            PatientId = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = DateTimeOffset.Now;
            HasDateOfBirth = false;
            Gender = string.Empty;
            Phone = string.Empty;
            EmergencyContact = string.Empty;
            ChiefComplaint = string.Empty;

            PatientIdError = string.Empty;
            FirstNameError = string.Empty;
            LastNameError = string.Empty;
            DateOfBirthError = string.Empty;
            PhoneError = string.Empty;
            EmergencyContactError = string.Empty;
            ChiefComplaintError = string.Empty;

            submitAttempted = false;
        }
    }
}