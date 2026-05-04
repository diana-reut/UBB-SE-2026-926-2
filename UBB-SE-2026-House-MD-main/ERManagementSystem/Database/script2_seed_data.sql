
USE HospitalManagementDb;
GO

-- PATIENT

INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('Ion', 'Popescu', '1800101123456', '1980-01-01', 'M', '+40712345678', '+40711111111', 0, 1),
('Ana', 'Ionescu', '2960505123456', '1996-05-05', 'F', '+40798765432', '+40797222222', 0, 0),
('Mihai', 'Georgescu', '5011225123456', '2001-12-25', 'M', '+40711112222', '+40709333333', 0, 1);
GO

-- MEDICAL HISTORY

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES
(1, 'A', 'Positive', 'Diabetes'),
(2, 'O', 'Negative', NULL),
(3, 'B', 'Positive', 'Asthma');
GO

-- ALLERGY

INSERT INTO Allergy (AllergyName, AllergyType, AllergyCategory)
VALUES
('Peanuts', 'Food', 'Food Allergy'),
('Pollen', 'Environmental', 'Seasonal'),
('Penicillin', 'Medication', 'Drug Allergy');
GO

-- TRANSPLANTS

INSERT INTO Transplants (ReceiverID, DonorID, OrganType, RequestDate, Status, CompatibilityScore)
VALUES
(1, 3, 'Kidney', GETDATE(), 'Pending', 85.5);
GO

-- MEDICAL RECORD

INSERT INTO MedicalRecord
(
    HistoryID,
    SourceType,
    SourceID,
    StaffID,
    Symptoms,
    Diagnosis,
    BasePrice,
    FinalPrice,
    DiscountApplied,
    PoliceNotified,
    TransplantID
)
VALUES
(1, 'ER Visit', 1, 101, 'Chest pain', 'Mild cardiac issue', 500.00, 450.00, 10, 0, NULL),
(2, 'Appointment', 2, 102, 'Headache', 'Migraine', 200.00, 200.00, NULL, 0, NULL),
(3, 'Appointment', 3, 103, 'Breathing difficulty', 'Asthma flare-up', 300.00, 270.00, 10, 0, 1);
GO

-- PATIENT ALLERGIES

INSERT INTO PatientAllergies (AllergyID, HistoryID, SeverityLevel)
VALUES
(1, 1, 'Severe'),
(2, 2, 'Moderate'),
(3, 3, 'Mild');
GO

-- PRESCRIPTION

INSERT INTO Prescription (RecordID, DoctorNotes)
VALUES
(1, 'Take medication daily after meals.'),
(2, 'Rest, hydration, and pain management.'),
(3, 'Use inhaler as needed and return for review.');
GO

-- PRESCRIPTION ITEMS

INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES
(1, 'Aspirin', '2/day'),
(1, 'Vitamin C', '1/day'),
(2, 'Ibuprofen', '1/day'),
(3, 'Ventolin', 'as needed');
GO