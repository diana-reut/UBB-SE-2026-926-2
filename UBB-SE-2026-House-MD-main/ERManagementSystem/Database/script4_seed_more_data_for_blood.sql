USE HospitalManagementDb;
GO

-- 1. INSERT 10 NEW TEST PATIENTS
INSERT INTO Patient (FirstName, LastName, CNP, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor)
VALUES 
('David', 'Popescu', '1890523123456', '1989-05-23', 'M', '0722123456', 'Maria Popescu', 0, 1),
('Elena', 'Ionescu', '2920815123456', '1992-08-15', 'F', '0733123456', 'Andrei Ionescu', 0, 1),
('Andrei', 'Munteanu', '1951102123456', '1995-11-02', 'M', '0744123456', 'Ioana Munteanu', 0, 0),
('Diana', 'Radu', '2850319123456', '1985-03-19', 'F', '0755123456', 'Mihai Radu', 0, 1),
('Cristian', 'Gheorghe', '1780707123456', '1978-07-07', 'M', '0766123456', 'Ana Gheorghe', 0, 1),
('Ana', 'Stan', '2991212123456', '1999-12-12', 'F', '0777123456', 'Vasile Stan', 0, 1),
('Bogdan', 'Marinescu', '1900101123456', '1990-01-01', 'M', '0788123456', 'Camelia Marinescu', 0, 0),
('Simona', 'Tudor', '2820404123456', '1982-04-04', 'F', '0799123456', 'Gabriel Tudor', 0, 1),
('Mihai', 'Dobre', '1930909123456', '1993-09-09', 'M', '0721111111', 'Luminita Dobre', 0, 1),
('Ioana', 'Lazar', '2880606123456', '1988-06-06', 'F', '0732222222', 'Radu Lazar', 0, 1);
GO

-- 2. CREATE MEDICAL HISTORIES FOR THE NEW PATIENTS
-- Note: 'Andrei' gets a ChronicCondition so he will fail the strict donor checks!
INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'A', 'Positive', NULL FROM Patient WHERE CNP = '1890523123456';

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'O', 'Negative', NULL FROM Patient WHERE CNP = '2920815123456'; -- PERFECT UNIVERSAL DONOR

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'B', 'Positive', 'Hypertension' FROM Patient WHERE CNP = '1951102123456'; -- BANNED: Chronic Condition

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'AB', 'Positive', NULL FROM Patient WHERE CNP = '2850319123456'; 

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'O', 'Positive', NULL FROM Patient WHERE CNP = '1780707123456';

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'A', 'Negative', NULL FROM Patient WHERE CNP = '2991212123456';

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'B', 'Negative', NULL FROM Patient WHERE CNP = '1900101123456';

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'AB', 'Negative', NULL FROM Patient WHERE CNP = '2820404123456';

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'O', 'Negative', NULL FROM Patient WHERE CNP = '1930909123456'; -- PERFECT UNIVERSAL DONOR

INSERT INTO MedicalHistory (PatientID, BloodType, RH, ChronicConditions)
SELECT PatientID, 'A', 'Positive', NULL FROM Patient WHERE CNP = '2880606123456';
GO

-- 3. LINK ALLERGIES SAFELY
-- Check your teammate's strict filter: 'Anaphylactic' allergy completely bans the patient.

-- David Popescu (A+): Has a mild allergy to Pollen. He WILL STILL APPEAR as a valid donor.
INSERT INTO PatientAllergies (AllergyID, HistoryID, SeverityLevel)
SELECT 
    (SELECT TOP 1 AllergyID FROM Allergy WHERE AllergyName = 'Pollen'),
    (SELECT TOP 1 HistoryID FROM MedicalHistory WHERE PatientID = (SELECT PatientID FROM Patient WHERE CNP = '1890523123456')),
    'Mild';

-- Diana Radu (AB+): Has a severe Anaphylactic allergy. She will be BANNED from being a donor.
INSERT INTO PatientAllergies (AllergyID, HistoryID, SeverityLevel)
SELECT 
    (SELECT TOP 1 AllergyID FROM Allergy WHERE AllergyName = 'Peanuts'),
    (SELECT TOP 1 HistoryID FROM MedicalHistory WHERE PatientID = (SELECT PatientID FROM Patient WHERE CNP = '2850319123456')),
    'Anaphylactic';
GO

