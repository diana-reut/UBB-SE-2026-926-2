USE HospitalManagementDb;
GO

UPDATE Patient 
SET EmergencyContact = '+40722111222' 
WHERE FirstName = 'Ion' AND LastName = 'Popescu';

UPDATE Patient 
SET EmergencyContact = '+40733444555' 
WHERE FirstName = 'Ana' AND LastName = 'Ionescu';

UPDATE Patient 
SET EmergencyContact = '+40744999888' 
WHERE FirstName = 'Mihai' AND LastName = 'Georgescu';
GO

ALTER TABLE Patient
ALTER COLUMN EmergencyContact VARCHAR(20);
GO