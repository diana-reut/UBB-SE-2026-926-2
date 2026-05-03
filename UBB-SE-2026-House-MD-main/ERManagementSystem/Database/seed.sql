-- ============================================
-- ER Management System - seed.sql
-- Sample data for local SQL Server
-- ============================================

USE ERManagementSystem;
GO

-- cleanup in FK-safe order
UPDATE dbo.ER_Room SET Current_Visit_ID = NULL;
DELETE FROM dbo.Transfer_Log;
DELETE FROM dbo.Examination;
DELETE FROM dbo.Triage_Parameters;
DELETE FROM dbo.Triage;
DELETE FROM dbo.ER_Visit;
DELETE FROM dbo.ER_Room;
DELETE FROM dbo.Patient;
GO

-- reseed ids to start from 1
DBCC CHECKIDENT ('dbo.ER_Room', RESEED, 0);
DBCC CHECKIDENT ('dbo.ER_Visit', RESEED, 0);
DBCC CHECKIDENT ('dbo.Triage', RESEED, 0);
DBCC CHECKIDENT ('dbo.Examination', RESEED, 0);
DBCC CHECKIDENT ('dbo.Transfer_Log', RESEED, 0);
GO

-- ============================================
-- 1. Patient (10 rows)
-- ============================================
INSERT INTO dbo.Patient
    (Patient_ID, First_Name, Last_Name, Date_of_Birth, Gender, Phone, Emergency_Contact, Transferred)
VALUES
    ('5010112123456', 'Andrei',   'Popescu',   '2001-01-01', 'Male',   '0745123897', 'Maria Popescu - 0726349812', 0),
    ('6020205123467', 'Elena',    'Ionescu',   '2002-02-05', 'Female', '0728437612', 'Mihai Ionescu - 0759213467', 0),
    ('1980315123478', 'Radu',     'Georgescu', '1998-03-15', 'Male',   '0736129845', 'Ana Georgescu - 0763458721', 0),
    ('2990412123489', 'Ioana',    'Dumitru',   '1999-04-12', 'Female', '0754389126', 'Sorin Dumitru - 0782195634', 0),
    ('1970525123490', 'Mihai',    'Stan',      '1997-05-25', 'Male',   '0769821345', 'Elena Stan - 0729183746', 0),
    ('6030618123501', 'Alexandra','Matei',     '2003-06-18', 'Female', '0782456139', 'Cristian Matei - 0746128937', 0),
    ('1960721123512', 'Vlad',     'Enache',    '1996-07-21', 'Male',   '0791348265', 'Laura Enache - 0738945612', 1),
    ('2950823123523', 'Bianca',   'Marin',     '1995-08-23', 'Female', '0725613984', 'Dan Marin - 0753498126', 0),
    ('6040915123534', 'Stefan',   'Ilie',      '2004-09-15', 'Male',   '0774129863', 'Camelia Ilie - 0795612348', 0),
    ('2931017123545', 'Teodora', 'Nistor', '1993-10-17', 'Female', '0738456129', 'Paul Nistor - 0749823156', 0);
GO

-- ============================================
-- 2. ER_Room (8 rows)
-- ============================================
INSERT INTO dbo.ER_Room
    (Room_Type, Availability_Status, Current_Visit_ID)
VALUES
    ('Operating Room (OR)', 'available', NULL),
    ('Operating Room (OR)', 'available', NULL),
    ('Trauma/Resuscitation Bay', 'occupied', NULL),       -- Will link to Visit 12
    ('Trauma/Resuscitation Bay', 'available', NULL),
    ('Respiratory/Monitored Room', 'occupied', NULL),      -- Will link to Visit 4
    ('Neurology/Quiet Observation Room', 'available', NULL),
    ('Orthopedic/Procedure Room', 'occupied', NULL),      -- Will link to Visit 15
    ('General Examination Room', 'occupied', NULL);        -- Will link to Visit 6
GO

-- ============================================
-- 3. ER_Visit (15 rows)
-- ============================================
INSERT INTO dbo.ER_Visit
    (Patient_ID, Arrival_date_time, Chief_Complaint, Status)
VALUES
    ('5010112123456', '2026-03-26 08:15:00', 'Severe abdominal pain',      'REGISTERED'),
    ('6020205123467', '2026-03-26 08:40:00', 'Shortness of breath',        'TRIAGED'),
    ('1980315123478', '2026-03-26 09:05:00', 'Head trauma after fall',     'WAITING_FOR_ROOM'),
    ('2990412123489', '2026-03-26 09:20:00', 'Chest pain',                 'IN_ROOM'),
    ('1970525123490', '2026-03-26 09:45:00', 'Broken arm',                 'WAITING_FOR_ROOM'),
    ('6030618123501', '2026-03-26 10:10:00', 'High fever and cough',       'IN_ROOM'),
    ('1960721123512', '2026-03-26 10:35:00', 'Dizziness',                  'TRANSFERRED'),
    ('2950823123523', '2026-03-26 11:00:00', 'Knee injury',                'CLOSED'),
    ('6040915123534', '2026-03-26 11:25:00', 'Loss of consciousness',      'TRIAGED'),
    ('2931017123545', '2026-03-26 11:50:00', 'Migraine',                   'REGISTERED'),
    ('5010112123456', '2026-03-27 07:30:00', 'Vomiting and dehydration',   'WAITING_FOR_ROOM'),
    ('6020205123467', '2026-03-27 07:50:00', 'Allergic reaction',          'IN_EXAMINATION'),
    ('1980315123478', '2026-03-27 08:10:00', 'Back pain',                  'REGISTERED'),
    ('2990412123489', '2026-03-27 08:30:00', 'Seizure episode',            'WAITING_FOR_ROOM'),
    ('2950823123523', '2026-03-27 08:45:00', 'Ankle sprain',               'IN_ROOM');
GO

-- ============================================
-- 4. Triage 
-- one triage per visit
-- ============================================
INSERT INTO dbo.Triage
    (Visit_ID, Triage_Level, Specialization, Nurse_ID, Triage_Time)
VALUES
    -- Visit 2: breathing=2 => Pulmonology, non-critical, score 16 => level 3
    (2,  3, 'Pulmonology',        101, '2026-03-26 08:50:00'),

    -- Visit 3: injury_type=3 => critical => level 1, specialization General Surgery
    (3,  1, 'General Surgery',    102, '2026-03-26 09:12:00'),

    -- Visit 4: non-critical general case => Emergency Medicine, score 11 => level 5
    (4,  5, 'Emergency Medicine', 103, '2026-03-26 09:28:00'),

    -- Visit 5: injury_type=2 => Orthopedics, non-critical, score 13 => level 4
    (5,  4, 'Orthopedics',        101, '2026-03-26 09:55:00'),

    -- Visit 6: breathing=2 => Pulmonology, non-critical, score 14 => level 4
    (6,  4, 'Pulmonology',        104, '2026-03-26 10:18:00'),

    -- Visit 7: consciousness=2 => Neurology, non-critical, score 14 => level 4
    (7,  4, 'Neurology',          102, '2026-03-26 10:42:00'),

    -- Visit 8: triaged then closed allowed; non-critical general case
    (8,  5, 'Orthopedics',        103, '2026-03-26 11:08:00'),

    -- Visit 9: consciousness=3 => critical => level 1, specialization Neurology
    (9,  1, 'Neurology',          104, '2026-03-26 11:33:00'),

    -- Visit 11: non-critical general case => Emergency Medicine, score 13 => level 4
    (11, 4, 'Emergency Medicine', 101, '2026-03-27 07:40:00'),

    -- Visit 12: breathing=2 => Pulmonology, non-critical, score 15 => level 4
    (12, 4, 'Pulmonology',        102, '2026-03-27 08:00:00'),

    -- Visit 14: consciousness=3 => critical => level 1, specialization Neurology
    (14, 1, 'Neurology',          103, '2026-03-27 08:38:00'),

    -- Visit 15: injury_type=2 => Orthopedics, non-critical, score 12 => level 4
    (15, 4, 'Orthopedics',        104, '2026-03-27 08:52:00');
GO

-- ============================================
-- 5. Triage_Parameters
-- Must match existing Triage_ID values
-- Since Triage_ID is IDENTITY and inserted in order above,
-- the generated IDs will be 1..12 in a fresh seed run
-- ============================================
INSERT INTO dbo.Triage_Parameters
    (Triage_ID, Consciousness, Breathing, Bleeding, Injury_Type, Pain_Level)
VALUES
    -- Visit 2
    (1, 1, 2, 1, 1, 3),
    -- Visit 3
    (2, 1, 2, 2, 3, 3),
    -- Visit 4
    (3, 1, 1, 1, 1, 1),
    -- Visit 5
    (4, 1, 1, 1, 2, 2),
    -- Visit 6
    (5, 1, 2, 1, 1, 2),
    -- Visit 7
    (6, 2, 1, 1, 1, 3),
    -- Visit 8
    (7, 1, 1, 1, 1, 1),
    -- Visit 9
    (8, 3, 1, 1, 1, 2),
    -- Visit 11
    (9, 1, 1, 2, 1, 2),
    -- Visit 12
    (10, 1, 2, 1, 1, 2),
    -- Visit 14
    (11, 3, 1, 1, 1, 1),
    -- Visit 15
    (12, 1, 1, 1, 2, 1);
GO

-- ============================================
-- 6. Examination 
-- ============================================
INSERT INTO dbo.Examination
    (Visit_ID, Doctor_ID, Exam_Time, Room_ID, Notes)
VALUES
    (8,  102, '2026-03-26 11:30:00', 4, 'Examined knee, minor contusion. Prescribed rest and ibuprofen.'),
    (12, 104, '2026-03-27 08:05:00', 3, 'Allergic reaction treated with antihistamines and monitored for airway stability.');
GO

-- ============================================
-- 7. Transfer_Log 
-- ============================================
INSERT INTO dbo.Transfer_Log
    (Visit_ID, Transfer_Time, Target_System, Status)
VALUES
-- Visit 7 - TRANSFERRED visit
-- Failed transfer (retry scenario)
    (7,  '2026-03-26 11:10:00', 'Patient Management', 'FAILED'),
-- Retry after failure => success
    (7, '2026-03-26 11:15:00', 'Patient Management', 'SUCCESS'),

-- Visit 12- IN_EXAMINATION
-- retry flow in progress
    (12, '2026-03-27 08:15:00', 'Patient Management', 'FAILED'),
    (12, '2026-03-27 08:20:00', 'Patient Management', 'RETRYING');
GO

-- ============================================
-- 8. Post-Seed Room Linkage
-- (Must be done after ER_Visit exists to satisfy FK_ER_Room_Current_Visit)
-- ============================================
UPDATE dbo.ER_Room SET Current_Visit_ID = 12 WHERE Room_ID = 3;
UPDATE dbo.ER_Room SET Current_Visit_ID = 4  WHERE Room_ID = 5;
UPDATE dbo.ER_Room SET Current_Visit_ID = 15 WHERE Room_ID = 7;
UPDATE dbo.ER_Room SET Current_Visit_ID = 6  WHERE Room_ID = 8;
GO