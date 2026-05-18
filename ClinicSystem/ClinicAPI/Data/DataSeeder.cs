using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Models;

namespace ClinicAPI.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context     = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply any pending migrations automatically
        await context.Database.MigrateAsync();

        // ── 1. Roles ────────────────────────────────────────────────────────
        string[] roles = ["Patient", "Doctor", "Receptionist", "ClinicManager"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── 2. Users ────────────────────────────────────────────────────────
        var manager       = await EnsureUserAsync(userManager, "manager@medcenter.com",       "Khalid",    "Al-Rashid", "Manager@123",      "ClinicManager");
        var doctor1       = await EnsureUserAsync(userManager, "doctor1@medcenter.com",       "Omar",      "Hassan",    "Doctor@123",       "Doctor");
        var doctor2       = await EnsureUserAsync(userManager, "doctor2@medcenter.com",       "Fatima",    "Al-Zahra",  "Doctor@123",       "Doctor");
        var doctor3       = await EnsureUserAsync(userManager, "doctor3@medcenter.com",       "Ahmed",     "Al-Nouri",  "Doctor@123",       "Doctor");
        var doctor4       = await EnsureUserAsync(userManager, "doctor4@medcenter.com",       "Sara",      "Khalifa",   "Doctor@123",       "Doctor");
        var doctor5       = await EnsureUserAsync(userManager, "doctor5@medcenter.com",       "Tariq",     "Al-Sayed",  "Doctor@123",       "Doctor");
        var doctor6       = await EnsureUserAsync(userManager, "doctor6@medcenter.com",       "Maryam",    "Jaffar",    "Doctor@123",       "Doctor");
        var receptionist  = await EnsureUserAsync(userManager, "receptionist@medcenter.com",  "Noor",      "Al-Amin",   "Recept@123",       "Receptionist");
        var patient1      = await EnsureUserAsync(userManager, "patient1@medcenter.com",      "Yousef",    "Mansoor",   "Patient@123",      "Patient");
        var patient2      = await EnsureUserAsync(userManager, "patient2@medcenter.com",      "Layla",     "Qassim",    "Patient@123",      "Patient");

        // ── 3. Specializations ──────────────────────────────────────────────
        if (!await context.Specializations.AnyAsync())
        {
            var specs = new List<Specialization>
            {
                new() { Name = "General Practice", Description = "Primary care and routine health checkups for all ages" },
                new() { Name = "Cardiology",       Description = "Heart and cardiovascular system conditions" },
                new() { Name = "Dermatology",      Description = "Skin, hair, and nail conditions" },
                new() { Name = "Pediatrics",       Description = "Medical care for infants, children, and adolescents" },
                new() { Name = "Orthopedics",      Description = "Bone, joint, muscle, and ligament conditions" },
                new() { Name = "Neurology",        Description = "Brain, spinal cord, and nervous system disorders" },
                new() { Name = "Ophthalmology",    Description = "Eye and vision care" },
                new() { Name = "ENT",              Description = "Ear, nose, and throat conditions" },
                new() { Name = "Psychiatry",       Description = "Mental health, behavioural, and emotional disorders" },
                new() { Name = "Gynecology",       Description = "Women's reproductive health and related conditions" }
            };
            await context.Specializations.AddRangeAsync(specs);
            await context.SaveChangesAsync();
        }

        // ── 4. Doctor Profiles ──────────────────────────────────────────────
        if (!await context.Doctors.AnyAsync())
        {
            var allSpecs = await context.Specializations.ToListAsync();
            var generalPractice = allSpecs.First(s => s.Name == "General Practice");
            var cardiology      = allSpecs.First(s => s.Name == "Cardiology");
            var pediatrics      = allSpecs.First(s => s.Name == "Pediatrics");
            var dermatology     = allSpecs.First(s => s.Name == "Dermatology");

            var doc1 = new Doctor
            {
                UserId        = doctor1!.Id,
                LicenseNumber = "BHR-DOC-2021-001",
                Bio           = "Senior cardiologist with 12 years of experience in interventional cardiology."
            };
            var doc2 = new Doctor
            {
                UserId        = doctor2!.Id,
                LicenseNumber = "BHR-DOC-2019-002",
                Bio           = "General practitioner specialising in paediatric care and family medicine."
            };
            var doc3 = new Doctor
            {
                UserId        = doctor3!.Id,
                LicenseNumber = "BHR-DOC-2020-003",
                Bio           = "Neurologist with 8 years of experience treating migraines, epilepsy, and stroke rehabilitation."
            };
            var doc4 = new Doctor
            {
                UserId        = doctor4!.Id,
                LicenseNumber = "BHR-DOC-2022-004",
                Bio           = "Ophthalmologist and ENT specialist with expertise in laser eye correction and sinus disorders."
            };
            var doc5 = new Doctor
            {
                UserId        = doctor5!.Id,
                LicenseNumber = "BHR-DOC-2018-005",
                Bio           = "Psychiatrist with over 10 years in cognitive behavioural therapy and anxiety management."
            };
            var doc6 = new Doctor
            {
                UserId        = doctor6!.Id,
                LicenseNumber = "BHR-DOC-2023-006",
                Bio           = "Gynaecologist and dermatologist focused on women's health and skin conditions."
            };

            await context.Doctors.AddRangeAsync(doc1, doc2, doc3, doc4, doc5, doc6);
            await context.SaveChangesAsync();

            var neurology    = allSpecs.First(s => s.Name == "Neurology");
            var ophthalmology = allSpecs.First(s => s.Name == "Ophthalmology");
            var ent          = allSpecs.First(s => s.Name == "ENT");
            var psychiatry   = allSpecs.First(s => s.Name == "Psychiatry");
            var gynecology   = allSpecs.First(s => s.Name == "Gynecology");

            // Doctor ↔ Specialization links
            await context.DoctorSpecializations.AddRangeAsync(
                new DoctorSpecialization { DoctorId = doc1.Id, SpecializationId = generalPractice.Id },
                new DoctorSpecialization { DoctorId = doc1.Id, SpecializationId = cardiology.Id },
                new DoctorSpecialization { DoctorId = doc2.Id, SpecializationId = generalPractice.Id },
                new DoctorSpecialization { DoctorId = doc2.Id, SpecializationId = pediatrics.Id },
                new DoctorSpecialization { DoctorId = doc3.Id, SpecializationId = neurology.Id },
                new DoctorSpecialization { DoctorId = doc3.Id, SpecializationId = generalPractice.Id },
                new DoctorSpecialization { DoctorId = doc4.Id, SpecializationId = ophthalmology.Id },
                new DoctorSpecialization { DoctorId = doc4.Id, SpecializationId = ent.Id },
                new DoctorSpecialization { DoctorId = doc5.Id, SpecializationId = psychiatry.Id },
                new DoctorSpecialization { DoctorId = doc6.Id, SpecializationId = gynecology.Id },
                new DoctorSpecialization { DoctorId = doc6.Id, SpecializationId = dermatology.Id }
            );

            // Weekly schedules
            var weekdays = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday
            };

            var schedules = new List<DoctorSchedule>();
            foreach (var day in weekdays)
            {
                // doc1 — Mon to Fri 08:00–17:00
                schedules.Add(new DoctorSchedule { DoctorId = doc1.Id, DayOfWeek = day, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 0, 0) });
                // doc2 — Mon to Fri 09:00–16:00
                schedules.Add(new DoctorSchedule { DoctorId = doc2.Id, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0) });
                // doc3 — Mon to Fri 08:00–14:00
                schedules.Add(new DoctorSchedule { DoctorId = doc3.Id, DayOfWeek = day, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0) });
                // doc5 — Mon to Fri 10:00–18:00
                schedules.Add(new DoctorSchedule { DoctorId = doc5.Id, DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0) });
            }

            // doc4 — Sun to Thu 08:00–15:00
            var sunToThu = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday };
            foreach (var day in sunToThu)
                schedules.Add(new DoctorSchedule { DoctorId = doc4.Id, DayOfWeek = day, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(15, 0, 0) });

            // doc6 — Mon, Wed, Fri 09:00–17:00 (part-time)
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
                schedules.Add(new DoctorSchedule { DoctorId = doc6.Id, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) });

            await context.DoctorSchedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
        }

        // ── 5. Patient Profiles ─────────────────────────────────────────────
        if (!await context.Patients.AnyAsync())
        {
            await context.Patients.AddRangeAsync(
                new Patient
                {
                    UserId           = patient1!.Id,
                    CPRNumber        = "860101001",
                    ReferenceNumber  = "PAT-0001",
                    DateOfBirth      = new DateTime(1986, 1, 1),
                    BloodType        = "O+",
                    EmergencyContact = "Ali Mansoor — +97366001001"
                },
                new Patient
                {
                    UserId           = patient2!.Id,
                    CPRNumber        = "920515002",
                    ReferenceNumber  = "PAT-0002",
                    DateOfBirth      = new DateTime(1992, 5, 15),
                    BloodType        = "A+",
                    EmergencyContact = "Hassan Qassim — +97366002002"
                }
            );
            await context.SaveChangesAsync();
        }

        // ── 6. Sample Appointments ──────────────────────────────────────────
        if (!await context.Appointments.AnyAsync())
        {
            var doc1        = await context.Doctors.FirstAsync();
            var pat1        = await context.Patients.FirstAsync();
            var pat2        = await context.Patients.Skip(1).FirstAsync();
            var generalSpec = await context.Specializations.FirstAsync(s => s.Name == "General Practice");
            var cardioSpec  = await context.Specializations.FirstAsync(s => s.Name == "Cardiology");

            // Appointment 1 — Confirmed (upcoming)
            var appt1 = new Appointment
            {
                PatientId           = pat1.Id,
                DoctorId            = doc1.Id,
                SpecializationId    = generalSpec.Id,
                AppointmentDateTime = DateTime.Today.AddDays(3).AddHours(10),
                Status              = AppointmentStatus.Confirmed,
                Notes               = "Routine annual checkup"
            };

            // Appointment 2 — Requested (upcoming)
            var appt2 = new Appointment
            {
                PatientId           = pat2.Id,
                DoctorId            = doc1.Id,
                SpecializationId    = cardioSpec.Id,
                AppointmentDateTime = DateTime.Today.AddDays(5).AddHours(14),
                Status              = AppointmentStatus.Requested,
                Notes               = "Chest pains — need cardiac evaluation"
            };

            await context.Appointments.AddRangeAsync(appt1, appt2);
            await context.SaveChangesAsync();

            // Status history for appointment 1
            await context.AppointmentStatusHistories.AddAsync(new AppointmentStatusHistory
            {
                AppointmentId   = appt1.Id,
                OldStatus       = AppointmentStatus.Requested,
                NewStatus       = AppointmentStatus.Confirmed,
                ChangedByUserId = pat1.UserId,
                ChangedAt       = DateTime.UtcNow.AddHours(-1)
            });
            await context.SaveChangesAsync();
        }
    }

    // ── Helper — create user only if they do not already exist ──────────────
    private static async Task<ApplicationUser?> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string firstName, string lastName,
        string password, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            // User already exists — reset their password to fix any bad/placeholder hash
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
            await userManager.ResetPasswordAsync(existing, resetToken, password);

            // Ensure the role is assigned even if it was missed
            if (!await userManager.IsInRoleAsync(existing, role))
                await userManager.AddToRoleAsync(existing, role);

            return existing;
        }

        var user = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            FirstName      = firstName,
            LastName       = lastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
