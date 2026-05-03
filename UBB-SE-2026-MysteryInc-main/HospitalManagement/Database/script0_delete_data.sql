USE HospitalManagementDb;
GO

-- ============================================
-- DELETE ALL DATA (in reverse dependency order)
-- ============================================
-- WARNING: This script will delete ALL data from the database
-- Execute this before running insert scripts to avoid duplicate constraint errors

PRINT 'Deleting data in reverse dependency order...'
GO

-- 1. Delete PrescriptionItems (depends on Prescription)
DELETE FROM PrescriptionItems;
PRINT 'Deleted PrescriptionItems'
GO

-- 2. Delete Prescription (depends on MedicalRecord)
DELETE FROM Prescription;
PRINT 'Deleted Prescription'
GO

-- 3. Delete PatientAllergies (depends on Allergy and MedicalHistory)
DELETE FROM PatientAllergies;
PRINT 'Deleted PatientAllergies'
GO

-- 4. Delete MedicalRecord (depends on MedicalHistory and Transplants)
DELETE FROM MedicalRecord;
PRINT 'Deleted MedicalRecord'
GO

-- 5. Delete Transplants (depends on Patient)
DELETE FROM Transplants;
PRINT 'Deleted Transplants'
GO

-- 6. Delete Allergy (no FK dependencies, but PatientAllergies referenced it)
DELETE FROM Allergy;
PRINT 'Deleted Allergy'
GO

-- 7. Delete MedicalHistory (depends on Patient)
DELETE FROM MedicalHistory;
PRINT 'Deleted MedicalHistory'
GO

-- 8. Delete Patient (root table)
DELETE FROM Patient;
PRINT 'Deleted Patient'
GO

-- ============================================
-- RESET IDENTITY SEEDS TO 1
-- ============================================
DBCC CHECKIDENT (Patient, RESEED, 0);
DBCC CHECKIDENT (MedicalHistory, RESEED, 0);
DBCC CHECKIDENT (Allergy, RESEED, 0);
DBCC CHECKIDENT (Transplants, RESEED, 0);
DBCC CHECKIDENT (MedicalRecord, RESEED, 0);
DBCC CHECKIDENT (Prescription, RESEED, 0);
DBCC CHECKIDENT (PrescriptionItems, RESEED, 0);
PRINT 'Identity seeds reset to 0'
GO

-- ============================================
-- VERIFY ALL TABLES ARE EMPTY
-- ============================================
PRINT ''
PRINT '=== FINAL VERIFICATION ==='
PRINT 'Patient count: '
SELECT COUNT(*) FROM Patient;
PRINT 'MedicalHistory count: '
SELECT COUNT(*) FROM MedicalHistory;
PRINT 'Allergy count: '
SELECT COUNT(*) FROM Allergy;
PRINT 'Transplants count: '
SELECT COUNT(*) FROM Transplants;
PRINT 'MedicalRecord count: '
SELECT COUNT(*) FROM MedicalRecord;
PRINT 'Prescription count: '
SELECT COUNT(*) FROM Prescription;
PRINT 'PrescriptionItems count: '
SELECT COUNT(*) FROM PrescriptionItems;
PRINT 'PatientAllergies count: '
SELECT COUNT(*) FROM PatientAllergies;
GO

PRINT ''
PRINT 'All tables cleared successfully. You can now run insert scripts.'
GO
