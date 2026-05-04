USE HospitalManagementDb;
GO

select * from MedicalRecord
-- ============================================
-- ADD TEST DATA FOR DRUG SHOPPING DETECTION
-- ============================================
-- This script creates patients exhibiting "Drug Shopping" behavior
-- to enable Pharmacist Dashboard "See All Addicts" functionality

-- ============================================
-- SCENARIO 1: SUSPICIOUS PATIENT - OXYCODONE DRUG SHOPPING
-- ============================================

-- Add Patient: Victor Marinov (Drug Shopping Suspect)
INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('Victor', 'Marinov', '1880322567890', '1988-03-22', 'M', '+40722999999', '+40720888888', 0, 0);

DECLARE @PatientID_Victor INT = IDENT_CURRENT('Patient');

-- Add Medical History for Victor
INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES
(@PatientID_Victor, 'O', 'Positive', 'Chronic Pain Disorder');

DECLARE @HistoryID_Victor INT = IDENT_CURRENT('MedicalHistory');

-- Add 4 Medical Records for Victor within last 30 days (all different doctors, same medication target)
-- Record 1: Dr. 201
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
    ConsultationDate
)
VALUES
(@HistoryID_Victor, 'ER Visit', 1, 201, 'Severe back pain', 'Acute lumbar pain', 400.00, 400.00, NULL, 0, DATEADD(day, -28, GETDATE()));

DECLARE @Record1_Victor INT = IDENT_CURRENT('MedicalRecord');

-- Record 2: Dr. 202
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
    ConsultationDate
)
VALUES
(@HistoryID_Victor, 'Appointment', 1, 202, 'Persistent back pain', 'Chronic lower back pain', 350.00, 350.00, NULL, 0, DATEADD(day, -21, GETDATE()));

DECLARE @Record2_Victor INT = IDENT_CURRENT('MedicalRecord');

-- Record 3: Dr. 203
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
    ConsultationDate
)
VALUES
(@HistoryID_Victor, 'ER Visit', 1, 203, 'Severe pain flare-up', 'Pain crisis', 420.00, 420.00, NULL, 0, DATEADD(day, -14, GETDATE()));

DECLARE @Record3_Victor INT = IDENT_CURRENT('MedicalRecord');

-- Record 4: Dr. 204
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
    ConsultationDate
)
VALUES
(@HistoryID_Victor, 'Appointment', 1, 204, 'Chronic pain management', 'Ongoing pain treatment', 380.00, 380.00, NULL, 0, DATEADD(day, -7, GETDATE()));

DECLARE @Record4_Victor INT = IDENT_CURRENT('MedicalRecord');

-- Add Prescriptions for Victor (all different doctors, same medication)
INSERT INTO Prescription (RecordID, DoctorNotes)
VALUES
(@Record1_Victor, 'For acute back pain. Take as needed.'),
(@Record2_Victor, 'Continue pain management.'),
(@Record3_Victor, 'Emergency pain relief prescribed.'),
(@Record4_Victor, 'Maintenance dose for chronic pain.');

-- Get the prescription IDs
DECLARE @Rx1_Victor INT;
DECLARE @Rx2_Victor INT;
DECLARE @Rx3_Victor INT;
DECLARE @Rx4_Victor INT;

SELECT @Rx1_Victor = PrescriptionID FROM Prescription WHERE RecordID = @Record1_Victor;
SELECT @Rx2_Victor = PrescriptionID FROM Prescription WHERE RecordID = @Record2_Victor;
SELECT @Rx3_Victor = PrescriptionID FROM Prescription WHERE RecordID = @Record3_Victor;
SELECT @Rx4_Victor = PrescriptionID FROM Prescription WHERE RecordID = @Record4_Victor;

-- Add Prescription Items (SAME MEDICATION: Oxycodone)
INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES
(@Rx1_Victor, 'Oxycodone', '30mg tablets, 30 tablets'),
(@Rx2_Victor, 'Oxycodone', '30mg tablets, 30 tablets'),
(@Rx3_Victor, 'Oxycodone', '30mg tablets, 40 tablets'),
(@Rx4_Victor, 'Oxycodone', '30mg tablets, 30 tablets');

PRINT 'SCENARIO 1 COMPLETE: Victor Marinov flagged for Oxycodone drug shopping (4 prescriptions, 4 different doctors, 30 days)'
GO

-- ============================================
-- SCENARIO 2: SUSPICIOUS PATIENT - ALPRAZOLAM DRUG SHOPPING
-- ============================================

-- Add Patient: Sergiu Popovici (Drug Shopping Suspect #2)
INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('Sergiu', 'Popovici', '1900615567891', '1990-06-15', 'M', '+40733888888', '+40731777777', 0, 0);

DECLARE @PatientID_Sergiu INT = IDENT_CURRENT('Patient');

-- Add Medical History for Sergiu
INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES
(@PatientID_Sergiu, 'A', 'Negative', 'Anxiety Disorder');

DECLARE @HistoryID_Sergiu INT = IDENT_CURRENT('MedicalHistory');

-- Add 5 Medical Records for Sergiu within last 30 days (all different doctors)
-- Record 1: Dr. 205
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
    ConsultationDate
)
VALUES
(@HistoryID_Sergiu, 'Appointment', 2, 205, 'Anxiety attack', 'Acute anxiety', 300.00, 300.00, NULL, 0, DATEADD(day, -29, GETDATE()));

DECLARE @Record1_Sergiu INT = IDENT_CURRENT('MedicalRecord');

-- Record 2: Dr. 206
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
    ConsultationDate
)
VALUES
(@HistoryID_Sergiu, 'ER Visit', 2, 206, 'Severe anxiety', 'Anxiety crisis', 350.00, 350.00, NULL, 0, DATEADD(day, -24, GETDATE()));

DECLARE @Record2_Sergiu INT = IDENT_CURRENT('MedicalRecord');

-- Record 3: Dr. 207
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
    ConsultationDate
)
VALUES
(@HistoryID_Sergiu, 'Appointment', 2, 207, 'Anxiety relapse', 'Recurrent anxiety disorder', 320.00, 320.00, NULL, 0, DATEADD(day, -18, GETDATE()));

DECLARE @Record3_Sergiu INT = IDENT_CURRENT('MedicalRecord');

-- Record 4: Dr. 208
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
    ConsultationDate
)
VALUES
(@HistoryID_Sergiu, 'ER Visit', 2, 208, 'Panic attack', 'Panic disorder', 360.00, 360.00, NULL, 0, DATEADD(day, -12, GETDATE()));

DECLARE @Record4_Sergiu INT = IDENT_CURRENT('MedicalRecord');

-- Record 5: Dr. 209
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
    ConsultationDate
)
VALUES
(@HistoryID_Sergiu, 'Appointment', 2, 209, 'Anxiety management', 'Chronic anxiety', 310.00, 310.00, NULL, 0, DATEADD(day, -5, GETDATE()));

DECLARE @Record5_Sergiu INT = IDENT_CURRENT('MedicalRecord');

-- Add Prescriptions for Sergiu (all different doctors, SAME MEDICATION)
INSERT INTO Prescription (RecordID, DoctorNotes)
VALUES
(@Record1_Sergiu, 'For acute anxiety symptoms.'),
(@Record2_Sergiu, 'Emergency prescription for severe anxiety.'),
(@Record3_Sergiu, 'Continue anxiety management.'),
(@Record4_Sergiu, 'Panic attack intervention.'),
(@Record5_Sergiu, 'Maintenance therapy.');

-- Get the prescription IDs
DECLARE @Rx1_Sergiu INT;
DECLARE @Rx2_Sergiu INT;
DECLARE @Rx3_Sergiu INT;
DECLARE @Rx4_Sergiu INT;
DECLARE @Rx5_Sergiu INT;

SELECT @Rx1_Sergiu = PrescriptionID FROM Prescription WHERE RecordID = @Record1_Sergiu;
SELECT @Rx2_Sergiu = PrescriptionID FROM Prescription WHERE RecordID = @Record2_Sergiu;
SELECT @Rx3_Sergiu = PrescriptionID FROM Prescription WHERE RecordID = @Record3_Sergiu;
SELECT @Rx4_Sergiu = PrescriptionID FROM Prescription WHERE RecordID = @Record4_Sergiu;
SELECT @Rx5_Sergiu = PrescriptionID FROM Prescription WHERE RecordID = @Record5_Sergiu;

-- Add Prescription Items (SAME MEDICATION: Alprazolam)
INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES
(@Rx1_Sergiu, 'Alprazolam', '1mg tablets, 30 tablets'),
(@Rx2_Sergiu, 'Alprazolam', '1mg tablets, 40 tablets'),
(@Rx3_Sergiu, 'Alprazolam', '1mg tablets, 30 tablets'),
(@Rx4_Sergiu, 'Alprazolam', '1mg tablets, 50 tablets'),
(@Rx5_Sergiu, 'Alprazolam', '1mg tablets, 30 tablets');

PRINT 'SCENARIO 2 COMPLETE: Sergiu Popovici flagged for Alprazolam drug shopping (5 prescriptions, 5 different doctors, 30 days)'
GO

-- ============================================
-- SCENARIO 3: LEGITIMATE PATIENT (Control Group)
-- ============================================

-- Add Patient: Cristian Albert (Legitimate multiple prescriptions for different medications)
INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('Cristian', 'Albert', '1910102567892', '1991-01-02', 'M', '+40744777777', '+40742666666', 0, 0);

DECLARE @PatientID_Cristian INT = IDENT_CURRENT('Patient');

-- Add Medical History for Cristian
INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES
(@PatientID_Cristian, 'B', 'Positive', 'Hypertension,Diabetes,High Cholesterol');

DECLARE @HistoryID_Cristian INT = IDENT_CURRENT('MedicalHistory');

-- Add 3 Medical Records for Cristian (legitimate - different medications for different conditions)
-- Record 1: Hypertension medication
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
    ConsultationDate
)
VALUES
(@HistoryID_Cristian, 'Appointment', 3, 210, 'High blood pressure', 'Hypertension', 280.00, 280.00, NULL, 0, DATEADD(day, -25, GETDATE()));

DECLARE @Record1_Cristian INT = IDENT_CURRENT('MedicalRecord');

-- Record 2: Diabetes management
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
    ConsultationDate
)
VALUES
(@HistoryID_Cristian, 'Appointment', 3, 211, 'Blood glucose spike', 'Type 2 Diabetes', 320.00, 320.00, NULL, 0, DATEADD(day, -15, GETDATE()));

DECLARE @Record2_Cristian INT = IDENT_CURRENT('MedicalRecord');

-- Record 3: Cholesterol management
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
    ConsultationDate
)
VALUES
(@HistoryID_Cristian, 'Appointment', 3, 212, 'High cholesterol levels', 'Hyperlipidemia', 260.00, 260.00, NULL, 0, DATEADD(day, -8, GETDATE()));

DECLARE @Record3_Cristian INT = IDENT_CURRENT('MedicalRecord');

-- Add Prescriptions for Cristian (DIFFERENT MEDICATIONS - legitimate)
INSERT INTO Prescription (RecordID, DoctorNotes)
VALUES
(@Record1_Cristian, 'Blood pressure control.'),
(@Record2_Cristian, 'Glucose management.'),
(@Record3_Cristian, 'Cholesterol reduction.');

-- Get the prescription IDs
DECLARE @Rx1_Cristian INT;
DECLARE @Rx2_Cristian INT;
DECLARE @Rx3_Cristian INT;

SELECT @Rx1_Cristian = PrescriptionID FROM Prescription WHERE RecordID = @Record1_Cristian;
SELECT @Rx2_Cristian = PrescriptionID FROM Prescription WHERE RecordID = @Record2_Cristian;
SELECT @Rx3_Cristian = PrescriptionID FROM Prescription WHERE RecordID = @Record3_Cristian;

-- Add Prescription Items (DIFFERENT MEDICATIONS - legitimate)
INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES
(@Rx1_Cristian, 'Lisinopril', '10mg tablets, 30 tablets'),
(@Rx2_Cristian, 'Metformin', '500mg tablets, 60 tablets'),
(@Rx3_Cristian, 'Simvastatin', '20mg tablets, 30 tablets');

PRINT 'SCENARIO 3 COMPLETE: Cristian Albert (legitimate patient - different medications for different conditions)'
GO

-- ============================================
-- VERIFICATION: Display Drug Shopping Candidates
-- ============================================

PRINT ''
PRINT '=== DRUG SHOPPING DETECTION TEST DATA SUMMARY ==='
PRINT ''
PRINT 'Run this query to see potential addict candidates:'
PRINT ''
PRINT 'SELECT 
    p.PatientID,
    p.FirstName,
    p.LastName,
    p.DateOfBirth,
    COUNT(DISTINCT pi.PrescriptionID) AS PrescriptionCount,
    COUNT(DISTINCT mr.StaffID) AS DifferentDoctors,
    pi.MedName AS MedicationName,
    MAX(mr.ConsultationDate) AS MostRecentDate
FROM Patient p
JOIN MedicalHistory mh ON p.PatientID = mh.PatientID
JOIN MedicalRecord mr ON mh.HistoryID = mr.HistoryID
JOIN Prescription prx ON mr.RecordID = prx.RecordID
JOIN PrescriptionItems pi ON prx.PrescriptionID = pi.PrescriptionID
WHERE mr.ConsultationDate >= DATEADD(day, -30, GETDATE())
GROUP BY p.PatientID, p.FirstName, p.LastName, p.DateOfBirth, pi.MedName
HAVING COUNT(DISTINCT pi.PrescriptionID) > 3 
    AND COUNT(DISTINCT mr.StaffID) > 2
    AND COUNT(DISTINCT pi.MedName) = 1
ORDER BY p.PatientID;'

PRINT ''
PRINT 'Expected Suspicious Patients:'
PRINT '1. Victor Marinov - Oxycodone (4 prescriptions from 4 different doctors)'
PRINT '2. Sergiu Popovici - Alprazolam (5 prescriptions from 5 different doctors)'
PRINT ''
PRINT 'Expected Legitimate Patient (SHOULD NOT BE FLAGGED):'
PRINT '3. Cristian Albert - Multiple different medications for different conditions'
GO
