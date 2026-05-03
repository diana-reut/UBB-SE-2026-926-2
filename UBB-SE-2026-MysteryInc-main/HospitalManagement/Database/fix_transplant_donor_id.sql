-- Fix: Allow NULL values for DonorID in Transplants table
-- This script allows transplant requests without a donor assigned initially

USE HospitalManagementDb;
GO

-- Drop the foreign key constraint first
ALTER TABLE Transplants
DROP CONSTRAINT FK_Transplants_Donor;
GO

-- Drop the check constraint that compares ReceiverID <> DonorID
ALTER TABLE Transplants
DROP CONSTRAINT CK_Transplants_DifferentPatients;
GO

-- Modify the DonorID column to allow NULL (if it's currently NOT NULL)
ALTER TABLE Transplants
ALTER COLUMN DonorID INT NULL;
GO

-- Recreate the foreign key constraint (it will allow NULL)
ALTER TABLE Transplants
ADD CONSTRAINT FK_Transplants_Donor
FOREIGN KEY (DonorID) REFERENCES Patient(PatientID);
GO

-- Recreate the check constraint to allow NULL DonorID
ALTER TABLE Transplants
ADD CONSTRAINT CK_Transplants_DifferentPatients
CHECK (DonorID IS NULL OR ReceiverID <> DonorID);
GO

PRINT 'Transplants table schema updated successfully. DonorID now allows NULL values.';
GO
