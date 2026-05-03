USE HospitalManagementDb;
GO


-- ============================================
-- INSERT ADDITIONAL PATIENTS
-- ============================================

INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('Cristian', 'Iacob', '1850715234567', '1985-07-15', 'M', '+40723456789', '+40722111111', 0, 1),
('Elena', 'Popa', '2900320234567', '1990-03-20', 'F', '+40745678901', '+40743222222', 0, 0),
('Adrian', 'Nicu', '1750505234567', '1975-05-05', 'M', '+40767890123', '+40765333333', 0, 1),
('Roxana', 'Dumitru', '2880812234567', '1988-08-12', 'F', '+40789012345', '+40787444444', 0, 0),
('Vasile', 'Stanescu', '1920428234567', '1992-04-28', 'M', '+40701234567', '+40699555555', 1, 0),
('Catalina', 'Marinescu', '2930915234567', '1993-09-15', 'F', '+40712121212', '+40710666666', 0, 1),
('Bogdan', 'Costescu', '1860706234567', '1986-07-06', 'M', '+40734343434', '+40732777777', 0, 0),
('Diana', 'Velasco', '2700111234567', '1970-01-11', 'F', '+40756565656', '+40754888888', 0, 1);
GO

-- ============================================
-- INSERT ADDITIONAL MEDICAL HISTORIES
-- ============================================

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
VALUES
(4, 'AB', 'Positive', 'Hypertension,High Cholesterol'),
(5, 'B', 'Negative', 'Type 2 Diabetes'),
(6, 'A', 'Positive', 'Asthma,GERD'),
(7, 'O', 'Positive', 'Arthritis'),
(8, 'AB', 'Negative', 'Chronic Kidney Disease'),
(9, 'B', 'Positive', 'Depression'),
(10, 'O', 'Negative', 'Hypothyroidism'),
(11, 'A', 'Negative', 'Migraine,Anxiety Disorder');
GO

-- ============================================
-- INSERT ADDITIONAL ALLERGIES
-- ============================================

INSERT INTO Allergy (AllergyName, AllergyType, AllergyCategory)
VALUES
('Shellfish', 'Food', 'Food Allergy'),
('Latex', 'Environmental', 'Contact Allergy'),
('Aspirin', 'Medication', 'Drug Allergy'),
('Bee Venom', 'Animal', 'Insect Sting Allergy'),
('Dust Mites', 'Environmental', 'Airborne Allergy'),
('Sulfa Drugs', 'Medication', 'Drug Allergy'),
('Egg', 'Food', 'Food Allergy'),
('Tree Nuts', 'Food', 'Food Allergy');
GO

-- ============================================
-- INSERT ADDITIONAL TRANSPLANTS
-- ============================================

INSERT INTO Transplants (ReceiverID, DonorID, OrganType, RequestDate, TransplantDate, Status, CompatibilityScore)
VALUES
(5, 6, 'Liver', DATEADD(day, -30, GETDATE()), DATEADD(day, -5, GETDATE()), 'Completed', 92.0),
(7, 4, 'Heart', DATEADD(day, -60, GETDATE()), NULL, 'Matched', 87.5),
(2, 9, 'Pancreas', DATEADD(day, -15, GETDATE()), NULL, 'Scheduled', 89.3),
(8, 11, 'Kidney', DATEADD(day, -10, GETDATE()), NULL, 'Pending', 78.5),
(10, NULL, 'Lung', DATEADD(day, -45, GETDATE()), NULL, 'Pending', 0.0);
GO

-- ============================================
-- INSERT ADDITIONAL PATIENT ALLERGIES
-- ============================================

INSERT INTO PatientAllergies (AllergyID, HistoryID, SeverityLevel)
VALUES
(4, 4, 'Moderate'),
(5, 5, 'Mild'),
(6, 6, 'Severe'),
(7, 7, 'Anaphylactic'),
(5, 8, 'Moderate'),
(8, 9, 'Mild'),
(4, 10, 'Severe'),
(6, 11, 'Mild');
GO

-- ============================================
-- INSERT ADDITIONAL MEDICAL RECORDS
-- ============================================

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
(4, 'ER Visit', 4, 104, 'High blood pressure', 'Hypertension crisis', 450.00, 405.00, 10, 0, NULL),
(5, 'Appointment', 5, 105, 'Excessive thirst', 'Blood glucose spike', 350.00, 350.00, NULL, 0, NULL),
(6, 'ER Visit', 6, 106, 'Severe cough', 'Asthma attack', 600.00, 540.00, 10, 0, NULL),
(7, 'Appointment', 7, 107, 'Joint pain', 'Osteoarthritis flare-up', 280.00, 280.00, NULL, 0, NULL),
(8, 'ER Visit', 8, 108, 'Nausea and fatigue', 'Kidney function decline', 750.00, 675.00, 10, 0, 5),
(9, 'Appointment', 9, 109, 'Sleep disorder', 'Depression with insomnia', 320.00, 320.00, NULL, 0, NULL),
(10, 'ER Visit', 10, 110, 'Weight gain', 'Thyroid dysfunction', 400.00, 360.00, 10, 0, NULL),
(11, 'Appointment', 11, 111, 'Severe headache', 'Migraine with aura', 550.00, 550.00, NULL, 0, NULL);
GO

-- ============================================
-- INSERT ADDITIONAL PRESCRIPTIONS
-- ============================================

INSERT INTO Prescription (RecordID, DoctorNotes)
VALUES
(4, 'Monitor blood pressure daily. Adjust medication if needed.'),
(5, 'Maintain low glycemic diet. Follow up in 2 weeks.'),
(6, 'Use nebulizer twice daily. Consult if symptoms persist.'),
(7, 'Physical therapy recommended. Anti-inflammatory medication.'),
(8, 'Schedule dialysis 3 times weekly. Monitor electrolytes.'),
(9, 'Continue antidepressant therapy. Consider sleep study.'),
(10, 'TSH levels to be checked monthly. Adjust levothyroxine if needed.'),
(11, 'Triptan as needed during migraine onset. Preventive therapy recommended.');
GO

-- ============================================
-- INSERT ADDITIONAL PRESCRIPTION ITEMS
-- ============================================

INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity)
VALUES
(4, 'Lisinopril', '1/day'),
(4, 'Simvastatin', '1/day'),
(5, 'Metformin', '2/day'),
(5, 'Glucose Monitor Strips', 'daily'),
(6, 'Methylprednisolone', 'as advised'),
(6, 'Albuterol Inhaler', 'as needed'),
(7, 'Ibuprofen', '2/day'),
(7, 'Calcium Citrate', '1/day'),
(8, 'Erythropoietin', '3x/week'),
(8, 'Phosphate Binder', 'with meals'),
(9, 'Sertraline', '1/day'),
(9, 'Melatonin Supplement', 'at bedtime'),
(10, 'Levothyroxine', '1/day morning'),
(11, 'Sumatriptan', 'as needed'),
(11, 'Propranolol', '1/day preventive');
GO

-- ============================================
-- VERIFICATION: Display the new data
-- ============================================

SELECT '=== PATIENT COUNT ===' AS Info;
SELECT COUNT(*) AS TotalPatients FROM Patient;

SELECT '=== MEDICAL HISTORY COUNT ===' AS Info;
SELECT COUNT(*) AS TotalMedicalHistories FROM MedicalHistory;

SELECT '=== ALLERGY COUNT ===' AS Info;
SELECT COUNT(*) AS TotalAllergies FROM Allergy;

SELECT '=== TRANSPLANT COUNT ===' AS Info;
SELECT COUNT(*) AS TotalTransplants FROM Transplants;

SELECT '=== MEDICAL RECORD COUNT ===' AS Info;
SELECT COUNT(*) AS TotalMedicalRecords FROM MedicalRecord;

SELECT '=== PRESCRIPTION COUNT ===' AS Info;
SELECT COUNT(*) AS TotalPrescriptions FROM Prescription;

SELECT '=== PRESCRIPTION ITEM COUNT ===' AS Info;
SELECT COUNT(*) AS TotalPrescriptionItems FROM PrescriptionItems;

-- Sample data display
SELECT '=== RECENT PATIENTS ===' AS Info;
SELECT TOP 10 PatientID, FirstName, LastName, DateOfBirth, IsDonor FROM Patient ORDER BY PatientID DESC;

SELECT '=== ACTIVE TRANSPLANTS ===' AS Info;
SELECT TransplantID, ReceiverID, DonorID, OrganType, Status, CompatibilityScore 
FROM Transplants 
WHERE Status IN ('Pending', 'Scheduled', 'Matched')
ORDER BY RequestDate DESC;
