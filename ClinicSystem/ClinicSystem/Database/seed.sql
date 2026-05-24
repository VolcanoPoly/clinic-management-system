-- ============================================================
-- SEED SCRIPT FOR CLINIC SYSTEM
-- Passwords for all users: [Role]@123  (e.g. Manager@123)
-- PasswordHash is a placeholder; generate real hashes via ASP.NET Core Identity
-- ============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

-- ============================================================
-- 0. Clean up existing seeded data (dependency order)
-- ============================================================
DELETE FROM PrescriptionItems;
DELETE FROM Prescriptions;
DELETE FROM VisitRecords;
DELETE FROM AppointmentStatusHistories;
DELETE FROM Appointments;
DELETE FROM DoctorSpecializations;
DELETE FROM DoctorSchedules;
DELETE FROM DoctorLeaves;
DELETE FROM Doctors;
DELETE FROM Patients;
DELETE FROM Notifications;
DELETE FROM Specializations;

-- ============================================================
-- 1. Declare User IDs
-- ============================================================
DECLARE @ManagerId      NVARCHAR(450) = NEWID();
DECLARE @Doctor1Id      NVARCHAR(450) = NEWID();
DECLARE @Doctor2Id      NVARCHAR(450) = NEWID();
DECLARE @ReceptionistId NVARCHAR(450) = NEWID();
DECLARE @Patient1Id     NVARCHAR(450) = NEWID();
DECLARE @Patient2Id     NVARCHAR(450) = NEWID();

-- ============================================================
-- 2. Insert Users (AspNetUsers)
-- ============================================================
INSERT INTO AspNetUsers
    (Id, UserName, NormalizedUserName, Email, NormalizedEmail,
     EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
     PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
     FirstName, LastName)
VALUES
    (@ManagerId,
     'manager@medcenter.com', 'MANAGER@MEDCENTER.COM',
     'manager@medcenter.com', 'MANAGER@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Khalid', 'Al-Rashid'),

    (@Doctor1Id,
     'doctor1@medcenter.com', 'DOCTOR1@MEDCENTER.COM',
     'doctor1@medcenter.com', 'DOCTOR1@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Omar', 'Hassan'),

    (@Doctor2Id,
     'doctor2@medcenter.com', 'DOCTOR2@MEDCENTER.COM',
     'doctor2@medcenter.com', 'DOCTOR2@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Fatima', 'Al-Zahra'),

    (@ReceptionistId,
     'receptionist@medcenter.com', 'RECEPTIONIST@MEDCENTER.COM',
     'receptionist@medcenter.com', 'RECEPTIONIST@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Noor', 'Al-Amin'),

    (@Patient1Id,
     'patient1@medcenter.com', 'PATIENT1@MEDCENTER.COM',
     'patient1@medcenter.com', 'PATIENT1@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Yousef', 'Mansoor'),

    (@Patient2Id,
     'patient2@medcenter.com', 'PATIENT2@MEDCENTER.COM',
     'patient2@medcenter.com', 'PATIENT2@MEDCENTER.COM',
     1, 'AQAAAAIAAYagAAAAEI1kX9XfLzP5Xz...Placeholder...', NEWID(), NEWID(),
     0, 0, 1, 0, 'Layla', 'Qassim');

-- ============================================================
-- 3. Assign Roles (AspNetUserRoles)
-- Role IDs: 1=Patient, 2=Doctor, 3=Receptionist, 4=ClinicManager
-- ============================================================
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES
    (@ManagerId,      '4'),
    (@Doctor1Id,      '2'),
    (@Doctor2Id,      '2'),
    (@ReceptionistId, '3'),
    (@Patient1Id,     '1'),
    (@Patient2Id,     '1');

-- ============================================================
-- 4. Insert Specializations
-- ============================================================
INSERT INTO Specializations (Name, Description)
VALUES
    ('General Practice',  'Primary care and routine health checkups for all ages'),
    ('Neurology',         'Diagnosis and treatment of nervous system disorders'),
    ('Orthopedics',       'Bone, joint, and musculoskeletal care'),
    ('Ophthalmology',     'Eye health, vision correction, and ocular surgery');

-- ============================================================
-- 5. Insert Doctor Profiles
-- ============================================================
INSERT INTO Doctors (UserId, LicenseNumber, Bio)
VALUES
    (@Doctor1Id, 'LIC-44201', 'Board-certified neurologist with 12 years of clinical experience.'),
    (@Doctor2Id, 'LIC-78530', 'Orthopedic surgeon specializing in sports injuries and joint replacement.');

-- ============================================================
-- 6. Link Doctors and Specializations
-- ============================================================
DECLARE @Doctor1TableId INT = (SELECT Id FROM Doctors WHERE UserId = @Doctor1Id);
DECLARE @Doctor2TableId INT = (SELECT Id FROM Doctors WHERE UserId = @Doctor2Id);

INSERT INTO DoctorSpecializations (DoctorId, SpecializationId)
VALUES
    (@Doctor1TableId, (SELECT Id FROM Specializations WHERE Name = 'Neurology')),
    (@Doctor1TableId, (SELECT Id FROM Specializations WHERE Name = 'General Practice')),
    (@Doctor2TableId, (SELECT Id FROM Specializations WHERE Name = 'Orthopedics')),
    (@Doctor2TableId, (SELECT Id FROM Specializations WHERE Name = 'General Practice'));

-- ============================================================
-- 7. Insert Doctor Schedules (Mon�Fri, 09:00�18:00)
-- ============================================================
INSERT INTO DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime)
SELECT @Doctor1TableId, v.Day, '09:00:00', '18:00:00'
FROM (VALUES (1),(2),(3),(4),(5)) AS v(Day);

INSERT INTO DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime)
SELECT @Doctor2TableId, v.Day, '09:00:00', '18:00:00'
FROM (VALUES (1),(2),(3),(4),(5)) AS v(Day);

-- ============================================================
-- 8. Insert Patient Profiles
-- ============================================================
INSERT INTO Patients (UserId, CPRNumber, ReferenceNumber, DateOfBirth, BloodType, EmergencyContact)
VALUES
    (@Patient1Id, '870312001', 'REF-2024-001', '1987-03-12', 'B+',  '39912001'),
    (@Patient2Id, '920720002', 'REF-2024-002', '1992-07-20', 'AB-', '39912002');

-- ============================================================
-- 9. Insert Appointments
-- ============================================================
DECLARE @Patient1TableId INT = (SELECT Id FROM Patients WHERE UserId = @Patient1Id);
DECLARE @Patient2TableId INT = (SELECT Id FROM Patients WHERE UserId = @Patient2Id);

INSERT INTO Appointments
    (PatientId, DoctorId, SpecializationId, AppointmentDateTime, Status, Notes)
VALUES
    -- Upcoming confirmed
    (@Patient1TableId, @Doctor1TableId,
     (SELECT Id FROM Specializations WHERE Name = 'Neurology'),
     DATEADD(day, 2, GETDATE()), 1, 'Follow-up for chronic migraines'),

    -- Pending
    (@Patient2TableId, @Doctor2TableId,
     (SELECT Id FROM Specializations WHERE Name = 'Orthopedics'),
     DATEADD(day, 4, GETDATE()), 0, 'Knee pain evaluation'),

    -- Completed
    (@Patient1TableId, @Doctor2TableId,
     (SELECT Id FROM Specializations WHERE Name = 'General Practice'),
     DATEADD(day, -3, GETDATE()), 4, 'Annual health screening'),

    -- Completed
    (@Patient2TableId, @Doctor1TableId,
     (SELECT Id FROM Specializations WHERE Name = 'General Practice'),
     DATEADD(day, -5, GETDATE()), 4, 'Blood pressure monitoring'),

    -- Upcoming pending
    (@Patient1TableId, @Doctor1TableId,
     (SELECT Id FROM Specializations WHERE Name = 'Neurology'),
     DATEADD(day, 7, GETDATE()), 0, 'EEG results review');

-- ============================================================
-- 10. Insert Visit Records for Completed Appointments
-- ============================================================
DECLARE @Appt3Id INT = (SELECT TOP 1 Id FROM Appointments
                        WHERE PatientId = @Patient1TableId AND Status = 4);
DECLARE @Appt4Id INT = (SELECT TOP 1 Id FROM Appointments
                        WHERE PatientId = @Patient2TableId AND Status = 4);

INSERT INTO VisitRecords (AppointmentId, DoctorNotes, Diagnosis, Treatment, CreatedAt)
VALUES
    (@Appt3Id,
     'Patient completed annual screening. All vitals within normal range.',
     'Healthy � no abnormalities detected',
     'Continue balanced diet and moderate exercise',
     DATEADD(day, -3, GETDATE())),

    (@Appt4Id,
     'Patient reported persistent elevated BP readings over past month.',
     'Stage 1 Hypertension',
     'Low-sodium diet, daily 30-minute walks, blood pressure log',
     DATEADD(day, -5, GETDATE()));

-- ============================================================
-- 11. Insert Prescriptions for Visit Records
-- ============================================================
DECLARE @Visit3Id INT = (SELECT Id FROM VisitRecords WHERE AppointmentId = @Appt3Id);
DECLARE @Visit4Id INT = (SELECT Id FROM VisitRecords WHERE AppointmentId = @Appt4Id);

INSERT INTO Prescriptions (VisitRecordId, DoctorId, IssuedAt)
VALUES
    (@Visit3Id, @Doctor2TableId, DATEADD(day, -3, GETDATE())),
    (@Visit4Id, @Doctor1TableId, DATEADD(day, -5, GETDATE()));

DECLARE @Presc3Id INT = (SELECT Id FROM Prescriptions WHERE VisitRecordId = @Visit3Id);
DECLARE @Presc4Id INT = (SELECT Id FROM Prescriptions WHERE VisitRecordId = @Visit4Id);

-- ============================================================
-- 12. Insert Prescription Items
-- ============================================================
INSERT INTO PrescriptionItems
    (PrescriptionId, MedicationName, Dosage, Frequency, Duration, Instructions)
VALUES
    -- For patient 1 (screening � vitamins)
    (@Presc3Id, 'Vitamin D3',   '1000 IU', 'Once daily',      '30 days', 'Take with breakfast'),
    (@Presc3Id, 'Omega-3',      '1000mg',  'Once daily',      '30 days', 'Take with a meal'),

    -- For patient 2 (hypertension)
    (@Presc4Id, 'Amlodipine',   '5mg',     'Once daily',      '30 days', 'Take in the morning'),
    (@Presc4Id, 'Hydrochlorothiazide', '12.5mg', 'Once daily', '30 days', 'Take with water, monitor BP daily');

-- ============================================================
-- 13. Insert Notifications
-- ============================================================
INSERT INTO Notifications (RecipientUserId, Message, IsRead, CreatedAt)
VALUES
    (@Patient1Id, 'Your appointment with Dr. Omar Hassan on '
        + CONVERT(NVARCHAR, DATEADD(day, 2, GETDATE()), 107)
        + ' has been confirmed.', 0, GETDATE()),

    (@Patient2Id, 'Your appointment request for Orthopedics is pending approval.', 0, GETDATE()),

    (@Doctor1Id,  'You have a new appointment request from patient Yousef Mansoor.', 0, GETDATE()),

    (@Doctor2Id,  'Reminder: 2 appointments scheduled for this week.', 1, DATEADD(day, -1, GETDATE())),

    (@ManagerId,  'System: Seed data loaded successfully. Review clinic roster.', 0, GETDATE());

-- ============================================================
COMMIT TRANSACTION;
GO