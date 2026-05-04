USE HospitalManagementDb;
GO

-- Find Ion Popescu's History ID
DECLARE @TargetHistoryID INT = (
    SELECT TOP 1 HistoryID 
    FROM MedicalHistory mh 
    JOIN Patient p ON mh.PatientID = p.PatientID 
    WHERE p.FirstName = 'Ion' AND p.LastName = 'Popescu'
);

-- Insert 11 ER Visits for today
INSERT INTO MedicalRecord (HistoryID, SourceType, SourceID, StaffID, ConsultationDate, BasePrice, FinalPrice, PoliceNotified)
VALUES 
(@TargetHistoryID, 'ER Visit', 1, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 2, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 3, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 4, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 5, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 6, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 7, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 8, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 9, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 10, 101, GETDATE(), 100, 100, 0),
(@TargetHistoryID, 'ER Visit', 11, 101, GETDATE(), 100, 100, 0);
GO
