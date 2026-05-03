-- ============================================
-- ER Management System - schema.sql
-- Hand-written SQL DDL for local SQL Server
-- ============================================

IF DB_ID('ERManagementSystem') IS NULL
BEGIN
    CREATE DATABASE ERManagementSystem;
END;
GO

USE ERManagementSystem;
GO

-- ============================================
-- 1. Patient
-- ============================================
IF OBJECT_ID('dbo.Patient', 'U') IS NOT NULL
    DROP TABLE dbo.Patient;
GO

CREATE TABLE dbo.Patient
(
    Patient_ID NVARCHAR(20) NOT NULL, -- CNP
    First_Name NVARCHAR(50) NOT NULL,
    Last_Name NVARCHAR(50) NOT NULL,
    Date_of_Birth DATE NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Emergency_Contact NVARCHAR(100) NOT NULL,
    Transferred BIT NOT NULL CONSTRAINT DF_Patient_Transferred DEFAULT 0,

    CONSTRAINT PK_Patient PRIMARY KEY (Patient_ID),
    CONSTRAINT CK_Patient_Gender CHECK (Gender IN ('Male', 'Female'))
);
GO

-- ============================================
-- 2. ER_Visit
-- ============================================
IF OBJECT_ID('dbo.ER_Visit', 'U') IS NOT NULL
    DROP TABLE dbo.ER_Visit;
GO

CREATE TABLE dbo.ER_Visit
(
    Visit_ID INT IDENTITY(1,1) NOT NULL,
    Patient_ID NVARCHAR(20) NOT NULL,
    Arrival_date_time DATETIME2 NOT NULL CONSTRAINT DF_ER_Visit_Arrival_Time DEFAULT SYSDATETIME(),
    Chief_Complaint NVARCHAR(255) NOT NULL,
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_ER_Visit PRIMARY KEY (Visit_ID),
    CONSTRAINT FK_ER_Visit_Patient
        FOREIGN KEY (Patient_ID) REFERENCES dbo.Patient(Patient_ID),
    CONSTRAINT CK_ER_Visit_Status
        CHECK (Status IN ('REGISTERED', 'TRIAGED', 'WAITING_FOR_ROOM', 'IN_ROOM', 'WAITING_FOR_DOCTOR', 'IN_EXAMINATION', 'TRANSFERRED', 'CLOSED'))
);
GO

-- ============================================
-- 3. Triage
-- ============================================
IF OBJECT_ID('dbo.Triage', 'U') IS NOT NULL
    DROP TABLE dbo.Triage;
GO

CREATE TABLE dbo.Triage
(
    Triage_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Triage_Level INT NOT NULL,
    Specialization NVARCHAR(50) NULL,
    Nurse_ID INT NOT NULL,
    Triage_Time DATETIME2 NOT NULL CONSTRAINT DF_Triage_Triage_Time DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Triage PRIMARY KEY (Triage_ID),
    CONSTRAINT FK_Triage_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES dbo.ER_Visit(Visit_ID),
    CONSTRAINT UQ_Triage_Visit UNIQUE (Visit_ID),
    CONSTRAINT CK_Triage_Level CHECK (Triage_Level BETWEEN 1 AND 5),
    CONSTRAINT CK_Specialization CHECK (
    Specialization IS NULL OR
    Specialization IN ('Orthopedics', 'Pulmonology', 'Neurology', 'General Surgery', 'Emergency Medicine'))
);
GO

-- ============================================
-- 4. Triage_Parameters
-- One-to-one with Triage
-- ============================================
IF OBJECT_ID('dbo.Triage_Parameters', 'U') IS NOT NULL
    DROP TABLE dbo.Triage_Parameters;
GO

CREATE TABLE dbo.Triage_Parameters
(
    Triage_ID INT NOT NULL,
    Consciousness INT NOT NULL,
    Breathing INT NOT NULL,
    Bleeding INT NOT NULL,
    Injury_Type INT NOT NULL,
    Pain_Level INT NOT NULL,

    CONSTRAINT PK_Triage_Parameters PRIMARY KEY (Triage_ID),
    CONSTRAINT FK_Triage_Parameters_Triage
        FOREIGN KEY (Triage_ID) REFERENCES dbo.Triage(Triage_ID),
    CONSTRAINT CK_Triage_Parameters_Consciousness CHECK (Consciousness BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Breathing CHECK (Breathing BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Bleeding CHECK (Bleeding BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Injury_Type CHECK (Injury_Type BETWEEN 1 AND 3),
    CONSTRAINT CK_Triage_Parameters_Pain_Level CHECK (Pain_Level BETWEEN 1 AND 3)
);
GO

-- ============================================
-- 5. ER_Room
-- ============================================
IF OBJECT_ID('dbo.ER_Room', 'U') IS NOT NULL
    DROP TABLE dbo.ER_Room;
GO

CREATE TABLE dbo.ER_Room
(
    Room_ID INT IDENTITY(1,1) NOT NULL,
    Room_Type NVARCHAR(50) NOT NULL,
    Availability_Status NVARCHAR(50) NOT NULL CONSTRAINT DF_ER_Room_Availability_Status DEFAULT 'available',
    Current_Visit_ID INT NULL,

    CONSTRAINT PK_ER_Room PRIMARY KEY (Room_ID),
    CONSTRAINT FK_ER_Room_ER_Visit FOREIGN KEY (Current_Visit_ID) REFERENCES dbo.ER_Visit(Visit_ID),
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

-- ============================================
-- 6. Examination
-- ============================================
IF OBJECT_ID('dbo.Examination', 'U') IS NOT NULL
    DROP TABLE dbo.Examination;
GO

CREATE TABLE dbo.Examination
(
    Exam_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Doctor_ID INT NOT NULL,
    Exam_Time DATETIME2 NOT NULL CONSTRAINT DF_Examination_Exam_Time DEFAULT SYSDATETIME(),
    Room_ID INT NOT NULL,
    Notes NVARCHAR(1000) NULL,

    CONSTRAINT PK_Examination PRIMARY KEY (Exam_ID),
    CONSTRAINT FK_Examination_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES dbo.ER_Visit(Visit_ID),
    CONSTRAINT FK_Examination_ER_Room
        FOREIGN KEY (Room_ID) REFERENCES dbo.ER_Room(Room_ID)
);
GO

-- ============================================
-- 7. Transfer_Log
-- ============================================
IF OBJECT_ID('dbo.Transfer_Log', 'U') IS NOT NULL
    DROP TABLE dbo.Transfer_Log;
GO

CREATE TABLE dbo.Transfer_Log
(
    Transfer_ID INT IDENTITY(1,1) NOT NULL,
    Visit_ID INT NOT NULL,
    Transfer_Time DATETIME2 NOT NULL CONSTRAINT DF_Transfer_Log_Transfer_Time DEFAULT SYSDATETIME(),
    Target_System NVARCHAR(30) NOT NULL,
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_Transfer_Log PRIMARY KEY (Transfer_ID),
    CONSTRAINT FK_Transfer_Log_ER_Visit
        FOREIGN KEY (Visit_ID) REFERENCES dbo.ER_Visit(Visit_ID),
    CONSTRAINT CK_Transfer_Log_Target_System
        CHECK (Target_System IN ('Patient Management')),
    CONSTRAINT CK_Transfer_Log_Status 
        CHECK (Status IN ('SUCCESS', 'FAILED', 'RETRYING'))
);
GO

USE ERManagementSystem;
ALTER TABLE dbo.Transfer_Log
ADD FilePath NVARCHAR(500) NULL