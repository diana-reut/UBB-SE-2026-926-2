using HospitalManagement.Entity;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.View.DialogServiceAdmin;

internal interface IDialogService
{
    public void SetWindow(Window window);

    public Task ShowAlertAsync(string message, string title = "System Message");

    public Task<bool> ShowConfirmAsync(string message, string title);

    public Task<DateTime?> ShowDatePickerAsync(string message, string title);
  
    public Task ShowOrganDonorDialogAsync(Patient patient);

    public Task<Patient?> ShowAddPatientDialogAsync();
   
    public Task<MedicalHistoryEntry> ShowMedicalHistoryAsync();
}
