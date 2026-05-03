using System;
using System.Collections.Generic;
using System.IO;
using HospitalManagement.Entity;
using iText.Kernel.Pdf;
using iText.Layout;
using Paragraph = iText.Layout.Element.Paragraph;

namespace HospitalManagement.Integration.Export;

internal static class PDFGenerator
{
    public static string GenerateRecordPDF(
        MedicalRecord record,
        Patient patient,
        Prescription? prescription,
        List<PrescriptionItem> items)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(patient);

        string fileName = $"MedicalRecord_{patient.FirstName}{patient.LastName}_{record.ConsultationDate:yyyyMMdd}.pdf";

        // THE FIX: Use standard .NET Desktop path! This prevents the "Operation is not valid" crash.
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, fileName);

        using (var writer = new PdfWriter(filePath))
        using (var pdf = new PdfDocument(writer))
        using (var doc = new Document(pdf))
        {
            _ = doc.Add(new Paragraph($"Patient: {patient.FirstName} {patient.LastName}").SetFontSize(16))
                .Add(new Paragraph($"CNP: {patient.Cnp}"))
                .Add(new Paragraph($"Consultation Date: {record.ConsultationDate:dd-MM-yyyy HH:mm}"))
                .Add(new Paragraph("\n"))
                .Add(new Paragraph("Section 1: Clinical Findings").SetFontSize(14))
                .Add(new Paragraph($"Symptoms: {record.Symptoms ?? "N/A"}"))
                .Add(new Paragraph($"Diagnosis: {record.Diagnosis ?? "N/A"}"))
                .Add(new Paragraph("\n"))
                .Add(new Paragraph("Section 2: Prescribed Treatment").SetFontSize(14));

            if (prescription is null || items.Count == 0)
            {
                _ = doc.Add(new Paragraph("No prescription issued for this consultation."));
            }
            else
            {
                _ = doc.Add(new Paragraph($"Doctor Notes: {prescription.DoctorNotes ?? "None"}"))
                    .Add(new Paragraph("Medications:"));
                foreach (PrescriptionItem item in items)
                {
                    _ = doc.Add(new Paragraph($"  - {item.MedName}: {item.Quantity}"));
                }
            }
        }

        return filePath;
    }
}
