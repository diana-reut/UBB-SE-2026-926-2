USE HospitalManagementDb;
GO

-- --- PACIENT: ANDREI SUSPECTU ---
BEGIN TRANSACTION;

INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES ('Andrei', 'Suspectu', '1990010199999', '1990-01-01', 'M', '+40722000111', 'Elena Suspectu', 0, 0);

DECLARE @AndreiID INT = SCOPE_IDENTITY();

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES (@AndreiID, 'AB', 'Positive', 'No documented chronic pain');

DECLARE @AndreiHistID INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@AndreiHistID, 'Appointment', 101, 101, 'Severe back pain', 'Acute pain', 100, 100, 0);
DECLARE @AR1 INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@AndreiHistID, 'Appointment', 102, 102, 'Unbearable back pain', 'Muscle spasm', 100, 100, 0);
DECLARE @AR2 INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@AndreiHistID, 'ER Visit', 103, 103, 'Needs pain meds', 'Lumbar pain', 200, 200, 0);
DECLARE @AR3 INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@AndreiHistID, 'Appointment', 104, 104, 'Back injury', 'Pain management', 100, 100, 0);
DECLARE @AR4 INT = SCOPE_IDENTITY();

-- Inserare Retete
INSERT INTO Prescription (RecordID, DoctorNotes) VALUES (@AR1, 'Urgent refill');
DECLARE @AP1 INT = SCOPE_IDENTITY();
INSERT INTO Prescription (RecordID, DoctorNotes) VALUES (@AR2, 'Patient requested X');
DECLARE @AP2 INT = SCOPE_IDENTITY();
INSERT INTO Prescription (RecordID, DoctorNotes) VALUES (@AR3, 'Second opinion pain');
DECLARE @AP3 INT = SCOPE_IDENTITY();
INSERT INTO Prescription (RecordID, DoctorNotes) VALUES (@AR4, 'Emergency refill');
DECLARE @AP4 INT = SCOPE_IDENTITY();

INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES (@AP1, 'Oxycodone', '20mg'), (@AP2, 'Oxycodone', '20mg'), (@AP3, 'Oxycodone', '20mg'), (@AP4, 'Oxycodone', '20mg');

COMMIT;
GO

-- --- PACIENT: MARIA IONESCU ---
BEGIN TRANSACTION;

INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES ('Maria', 'Ionescu', '2850505223344', '1985-05-05', 'F', '+40733111222', 'George Ionescu', 0, 0);

DECLARE @MariaID INT = SCOPE_IDENTITY();

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES (@MariaID, 'O', 'Positive', 'Chronic Lower Back Pain (L4-L5 Herniated Disc), History of Spinal Surgery 2022.');

DECLARE @MariaHistID INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@MariaHistID, 'Appointment', 201, 101, 'Sharp back pain', 'Chronic pain flare-up', 150, 150, 0);
DECLARE @MR1 INT = SCOPE_IDENTITY();

INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, BasePrice, FinalPrice, PoliceNotified)
VALUES (@MariaHistID, 'ER Visit', 202, 102, 'Cannot walk due to pain', 'Radiculopathy', 300, 300, 0);
DECLARE @MR2 INT = SCOPE_IDENTITY();

COMMIT;
GO