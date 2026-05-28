using System;
using System.Collections.Generic;

namespace Common.Data.Entity.DTOs;

public class CreatePrescriptionDto
{
    public string? DoctorNotes { get; set; }
    public DateTime Date { get; set; }
    public List<CreatePrescriptionItemDto> Items { get; set; } = new List<CreatePrescriptionItemDto>();
}
