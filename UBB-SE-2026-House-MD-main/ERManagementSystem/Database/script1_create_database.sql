IF DB_ID('HospitalManagementDb') IS NULL
BEGIN
    CREATE DATABASE HospitalManagementDb;
END
GO

USE HospitalManagementDb;
GO

-- Drop tables in reverse dependency order

IF OBJECT_ID('PrescriptionItems', 'U') IS NOT NULL
    DROP TABLE PrescriptionItems;
GO

IF OBJECT_ID('Prescription', 'U') IS NOT NULL
    DROP TABLE Prescription;
GO

IF OBJECT_ID('PatientAllergies', 'U') IS NOT NULL
    DROP TABLE PatientAllergies;
GO

IF OBJECT_ID('MedicalRecord', 'U') IS NOT NULL
    DROP TABLE MedicalRecord;
GO

IF OBJECT_ID('Transplants', 'U') IS NOT NULL
    DROP TABLE Transplants;
GO

IF OBJECT_ID('Allergy', 'U') IS NOT NULL
    DROP TABLE Allergy;
GO

IF OBJECT_ID('MedicalHistory', 'U') IS NOT NULL
    DROP TABLE MedicalHistory;
GO

IF OBJECT_ID('Transfer_Log', 'U') IS NOT NULL
    DROP TABLE Transfer_Log;
GO

IF OBJECT_ID('Examination', 'U') IS NOT NULL
    DROP TABLE Examination;
GO

IF OBJECT_ID('Triage_Parameters', 'U') IS NOT NULL
    DROP TABLE Triage_Parameters;
GO

IF OBJECT_ID('Triage', 'U') IS NOT NULL
    DROP TABLE Triage;
GO

IF OBJECT_ID('ER_Room', 'U') IS NOT NULL
    DROP TABLE ER_Room;
GO

IF OBJECT_ID('ER_Visit', 'U') IS NOT NULL
    DROP TABLE ER_Visit;
GO

IF OBJECT_ID('Patient', 'U') IS NOT NULL
    DROP TABLE Patient;
GO

-- Patient

CREATE TABLE Patient
(
    PatientID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    CNP CHAR(13) NOT NULL,
    DateOfBirth DATE NOT NULL,
    DateOfDeath DATE NULL,
    Sex CHAR(1) NOT NULL,
    Phone VARCHAR(20) NULL,
    EmergencyContact VARCHAR(255) NULL,
    Archived BIT NOT NULL DEFAULT 0,
    IsDonor BIT NOT NULL DEFAULT 0,
    Transferred BIT NOT NULL DEFAULT 0,

    CONSTRAINT UQ_Patient_CNP UNIQUE (CNP),

    CONSTRAINT CK_Patient_CNP
        CHECK (
            LEN(CNP) = 13
            AND CNP NOT LIKE '%[^0-9]%'
            AND LEFT(CNP, 1) IN ('1', '2', '5', '6')
        ),

    CONSTRAINT CK_Patient_Sex
        CHECK (Sex IN ('M', 'F')),

    CONSTRAINT CK_Patient_DateOfDeath
        CHECK (DateOfDeath IS NULL OR DateOfDeath >= DateOfBirth)
);
GO

CREATE NONCLUSTERED INDEX IX_Patient_LastName_FirstName
ON Patient(LastName, FirstName);
GO

-- MedicalHistory

CREATE TABLE MedicalHistory
(
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    BloodType VARCHAR(3) NULL,
    RH VARCHAR(10) NULL,
    ChronicConditions VARCHAR(2000) NULL,

    CONSTRAINT UQ_MedicalHistory_PatientID UNIQUE (PatientID),

    CONSTRAINT FK_MedicalHistory_Patient
        FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),

    CONSTRAINT CK_MedicalHistory_BloodType
        CHECK (BloodType IS NULL OR BloodType IN ('A', 'B', 'AB', 'O')),

    CONSTRAINT CK_MedicalHistory_RH
        CHECK (RH IS NULL OR RH IN ('Positive', 'Negative'))
);
GO

-- Allergy

CREATE TABLE Allergy
(
    AllergyID INT IDENTITY(1,1) PRIMARY KEY,
    AllergyName VARCHAR(100) NOT NULL,
    AllergyType VARCHAR(100) NULL,
    AllergyCategory VARCHAR(100) NULL,

    CONSTRAINT UQ_Allergy_AllergyName UNIQUE (AllergyName),

    CONSTRAINT CK_Allergy_Name_NotEmpty
        CHECK (LEN(LTRIM(RTRIM(AllergyName))) > 0)
);
GO

-- Transplants

CREATE TABLE Transplants
(
    TransplantID INT IDENTITY(1,1) PRIMARY KEY,
    ReceiverID INT NOT NULL,
    DonorID INT NULL,
    OrganType VARCHAR(100) NOT NULL,
    RequestDate DATETIME NOT NULL,
    TransplantDate DATETIME NULL,
    Status VARCHAR(50) NOT NULL,
    CompatibilityScore FLOAT NOT NULL,

    CONSTRAINT FK_Transplants_Receiver
        FOREIGN KEY (ReceiverID) REFERENCES Patient(PatientID),

    CONSTRAINT FK_Transplants_Donor
        FOREIGN KEY (DonorID) REFERENCES Patient(PatientID),

    CONSTRAINT CK_Transplants_DifferentPatients
        CHECK (ReceiverID <> DonorID),

    CONSTRAINT CK_Transplants_Status
        CHECK (Status IN ('Pending', 'Matched', 'Scheduled', 'Completed', 'Cancelled')),

    CONSTRAINT CK_Transplants_CompatibilityScore
        CHECK (CompatibilityScore >= 0 AND CompatibilityScore <= 100),

    CONSTRAINT CK_Transplants_TransplantDate
        CHECK (TransplantDate IS NULL OR TransplantDate >= RequestDate)
);
GO

CREATE NONCLUSTERED INDEX IX_Transplants_ReceiverID
ON Transplants(ReceiverID);
GO

CREATE NONCLUSTERED INDEX IX_Transplants_DonorID
ON Transplants(DonorID);
GO

CREATE NONCLUSTERED INDEX IX_Transplants_Status
ON Transplants(Status);
GO

-- MedicalRecord

CREATE TABLE MedicalRecord
(
    RecordID INT IDENTITY(1,1) PRIMARY KEY,
    HistoryID INT NOT NULL,
    SourceType VARCHAR(50) NOT NULL,
    SourceID INT NOT NULL,
    StaffID INT NOT NULL,
    Symptoms VARCHAR(500) NULL,
    Diagnosis VARCHAR(500) NULL,
    ConsultationDate DATETIME NOT NULL DEFAULT GETDATE(),
    BasePrice DECIMAL(18,2) NOT NULL,
    FinalPrice DECIMAL(18,2) NOT NULL,
    DiscountApplied INT NULL,
    PoliceNotified BIT NOT NULL DEFAULT 0,
    TransplantID INT NULL,

    CONSTRAINT FK_MedicalRecord_MedicalHistory
        FOREIGN KEY (HistoryID) REFERENCES MedicalHistory(HistoryID),

    CONSTRAINT FK_MedicalRecord_Transplants
        FOREIGN KEY (TransplantID) REFERENCES Transplants(TransplantID),

    CONSTRAINT CK_MedicalRecord_SourceType
        CHECK (SourceType IN ('ER Visit', 'Appointment')),

    CONSTRAINT CK_MedicalRecord_BasePrice
        CHECK (BasePrice >= 0),

    CONSTRAINT CK_MedicalRecord_FinalPrice
        CHECK (FinalPrice >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_MedicalRecord_HistoryID
ON MedicalRecord(HistoryID);
GO

CREATE NONCLUSTERED INDEX IX_MedicalRecord_ConsultationDate
ON MedicalRecord(ConsultationDate);
GO

-- PatientAllergies

CREATE TABLE PatientAllergies
(
    AllergyID INT NOT NULL,
    HistoryID INT NOT NULL,
    SeverityLevel VARCHAR(50) NOT NULL,

    CONSTRAINT PK_PatientAllergies
        PRIMARY KEY (AllergyID, HistoryID),

    CONSTRAINT FK_PatientAllergies_Allergy
        FOREIGN KEY (AllergyID) REFERENCES Allergy(AllergyID),

    CONSTRAINT FK_PatientAllergies_MedicalHistory
        FOREIGN KEY (HistoryID) REFERENCES MedicalHistory(HistoryID),

    CONSTRAINT CK_PatientAllergies_SeverityLevel
        CHECK (SeverityLevel IN ('Mild', 'Moderate', 'Severe', 'Anaphylactic'))
);
GO

CREATE NONCLUSTERED INDEX IX_PatientAllergies_HistoryID
ON PatientAllergies(HistoryID);
GO


-- Prescription

CREATE TABLE Prescription
(
    PrescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    RecordID INT NOT NULL,
    DoctorNotes VARCHAR(1000) NULL,
    [Date] DATE NOT NULL DEFAULT CONVERT(DATE, GETDATE()),

    CONSTRAINT UQ_Prescription_RecordID UNIQUE (RecordID),

    CONSTRAINT FK_Prescription_MedicalRecord
        FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID)
);
GO

CREATE NONCLUSTERED INDEX IX_Prescription_RecordID
ON Prescription(RecordID);
GO


-- PrescriptionItems

CREATE TABLE PrescriptionItems
(
    PrescrItemID INT IDENTITY(1,1) PRIMARY KEY,
    PrescriptionID INT NOT NULL,
    MedName VARCHAR(150) NOT NULL,
    Quantity VARCHAR(50) NULL,

    CONSTRAINT FK_PrescriptionItems_Prescription
        FOREIGN KEY (PrescriptionID) REFERENCES Prescription(PrescriptionID)
);
GO

CREATE NONCLUSTERED INDEX IX_PrescriptionItems_PrescriptionID
ON PrescriptionItems(PrescriptionID);
GO

-- ER_Visit

CREATE TABLE ER_Visit
(
    Visit_ID INT IDENTITY(1,1) NOT NULL,
    Patient_ID CHAR(13) NOT NULL,
    Arrival_date_time DATETIME2 NOT NULL CONSTRAINT DF_ER_Visit_Arrival_Time DEFAULT SYSDATETIME(),
    Chief_Complaint NVARCHAR(255) NOT NULL,
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_ER_Visit PRIMARY KEY (Visit_ID),
    CONSTRAINT FK_ER_Visit_Patient
        FOREIGN KEY (Patient_ID) REFERENCES Patient(CNP),
    CONSTRAINT CK_ER_Visit_Status
        CHECK (Status IN ('REGISTERED', 'TRIAGED', 'WAITING_FOR_ROOM', 'IN_ROOM', 'WAITING_FOR_DOCTOR', 'IN_EXAMINATION', 'TRANSFERRED', 'CLOSED'))
);
GO

CREATE NONCLUSTERED INDEX IX_ER_Visit_Patient_ID
ON ER_Visit(Patient_ID);
GO

-- Triage

CREATE TABLE Triage
(
    Triage_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Triage_Level INT NOT NULL,
    Specialization NVARCHAR(50) NULL,
    Nurse_ID INT NOT NULL,
    Triage_Time DATETIME2 NOT NULL CONSTRAINT DF_Triage_Triage_Time DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Triage PRIMARY KEY (Triage_ID),
    CONSTRAINT FK_Triage_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES ER_Visit(Visit_ID),
    CONSTRAINT UQ_Triage_Visit UNIQUE (Visit_ID),
    CONSTRAINT CK_Triage_Level CHECK (Triage_Level BETWEEN 1 AND 5),
    CONSTRAINT CK_Specialization CHECK (
        Specialization IS NULL OR
        Specialization IN ('Orthopedics', 'Pulmonology', 'Neurology', 'General Surgery', 'Emergency Medicine'))
);
GO

-- Triage_Parameters

CREATE TABLE Triage_Parameters
(
    Triage_ID INT NOT NULL,
    Consciousness INT NOT NULL,
    Breathing INT NOT NULL,
    Bleeding INT NOT NULL,
    Injury_Type INT NOT NULL,
    Pain_Level INT NOT NULL,

    CONSTRAINT PK_Triage_Parameters PRIMARY KEY (Triage_ID),
    CONSTRAINT FK_Triage_Parameters_Triage
        FOREIGN KEY (Triage_ID) REFERENCES Triage(Triage_ID),
    CONSTRAINT CK_Triage_Parameters_Consciousness CHECK (Consciousness BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Breathing CHECK (Breathing BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Bleeding CHECK (Bleeding BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Injury_Type CHECK (Injury_Type BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Pain_Level CHECK (Pain_Level BETWEEN 1 AND 3)
);
GO

-- ER_Room

CREATE TABLE ER_Room
(
    Room_ID INT IDENTITY(1,1) NOT NULL,
    Room_Type NVARCHAR(50) NOT NULL,
    Availability_Status NVARCHAR(50) NOT NULL CONSTRAINT DF_ER_Room_Availability_Status DEFAULT 'available',
    Current_Visit_ID INT NULL,

    CONSTRAINT PK_ER_Room PRIMARY KEY (Room_ID),
    CONSTRAINT FK_ER_Room_ER_Visit FOREIGN KEY (Current_Visit_ID) REFERENCES ER_Visit(Visit_ID),
    CONSTRAINT CK_ER_Room_Room_Type
        CHECK (Room_Type IN (
            'Operating Room (OR)',
            'Trauma/Resuscitation Bay',
            'Respiratory/Monitored Room',
            'Neurology/Quiet Observation Room',
            'Orthopedic/Procedure Room',
            'General Examination Room'
        )),
    CONSTRAINT CK_ER_Room_Availability_Status
        CHECK (Availability_Status IN ('available', 'occupied', 'cleaning'))
);
GO

-- Examination

CREATE TABLE Examination
(
    Exam_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Doctor_ID INT NOT NULL,
    Exam_Time DATETIME2 NOT NULL CONSTRAINT DF_Examination_Exam_Time DEFAULT SYSDATETIME(),
    Room_ID INT NOT NULL,
    Notes NVARCHAR(1000) NULL,

    CONSTRAINT PK_Examination PRIMARY KEY (Exam_ID),
    CONSTRAINT FK_Examination_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES ER_Visit(Visit_ID),
    CONSTRAINT FK_Examination_ER_Room
        FOREIGN KEY (Room_ID) REFERENCES ER_Room(Room_ID)
);
GO

-- Transfer_Log

CREATE TABLE Transfer_Log
(
    Transfer_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Transfer_Time DATETIME2 NOT NULL CONSTRAINT DF_Transfer_Log_Transfer_Time DEFAULT SYSDATETIME(),
    Target_System NVARCHAR(30) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    FilePath NVARCHAR(500) NULL,

    CONSTRAINT PK_Transfer_Log PRIMARY KEY (Transfer_ID),
    CONSTRAINT FK_Transfer_Log_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES ER_Visit(Visit_ID),
    CONSTRAINT CK_Transfer_Log_Target_System
        CHECK (Target_System IN ('Patient Management')),
    CONSTRAINT CK_Transfer_Log_Status
        CHECK (Status IN ('SUCCESS', 'FAILED', 'RETRYING'))
);
GO
