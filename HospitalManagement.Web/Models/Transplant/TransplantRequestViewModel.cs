using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models.Transplant;

public class TransplantRequestViewModel
{
    [Required]
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public bool IsUrgent { get; set; }

    public string? WarningMessage { get; set; }

    [Required(ErrorMessage = "Please select an organ type.")]
    public string? SelectedOrgan { get; set; }
    public List<string> AvailableOrgans { get; } = new ()
    {
        "Kidney", "Heart", "Liver", "Lung", "Pancreas", "Cornea"
    };
}