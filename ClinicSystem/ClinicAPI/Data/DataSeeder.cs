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

        // ── 3. Specializations — add any that are missing by name ───────────
        var allSpecNames = new[]
        {
            ("General Practice", "Primary care and routine health checkups for all ages"),
            ("Cardiology",       "Heart and cardiovascular system conditions"),
            ("Dermatology",      "Skin, hair, and nail conditions"),
            ("Pediatrics",       "Medical care for infants, children, and adolescents"),
            ("Orthopedics",      "Bone, joint, muscle, and ligament conditions"),
            ("Neurology",        "Brain, spinal cord, and nervous system disorders"),
            ("Ophthalmology",    "Eye and vision care"),
            ("ENT",              "Ear, nose, and throat conditions"),
            ("Psychiatry",       "Mental health, behavioural, and emotional disorders"),
            ("Gynecology",       "Women's reproductive health and related conditions")
        };
        foreach (var (name, desc) in allSpecNames)
        {
            if (!await context.Specializations.AnyAsync(s => s.Name == name))
            {
                context.Specializations.Add(new Specialization { Name = name, Description = desc });
            }
        }
        await context.SaveChangesAsync();

        // ── 4. Doctor Profiles — add any that are missing by UserId ─────────
        var allSpecs      = await context.Specializations.ToListAsync();
        var specByName    = allSpecs.ToDictionary(s => s.Name);

        async Task<Doctor> EnsureDoctorAsync(ApplicationUser? user, string license, string bio,
            DayOfWeek[] days, TimeSpan start, TimeSpan end, string[] specializationNames)
        {
            var existing = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
            if (existing != null) return existing;

            var doc = new Doctor { UserId = user!.Id, LicenseNumber = license, Bio = bio };
            context.Doctors.Add(doc);
            await context.SaveChangesAsync();

            // Specialization links
            foreach (var specName in specializationNames)
            {
                if (specByName.TryGetValue(specName, out var spec) &&
                    !await context.DoctorSpecializations.AnyAsync(ds => ds.DoctorId == doc.Id && ds.SpecializationId == spec.Id))
                {
                    context.DoctorSpecializations.Add(new DoctorSpecialization { DoctorId = doc.Id, SpecializationId = spec.Id });
                }
            }

            // Schedules
            foreach (var day in days)
                context.DoctorSchedules.Add(new DoctorSchedule { DoctorId = doc.Id, DayOfWeek = day, StartTime = start, EndTime = end });

            await context.SaveChangesAsync();
            return doc;
        }

        var weekdays  = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var sunToThu  = new[] { DayOfWeek.Sunday, DayOfWeek.Monday,  DayOfWeek.Tuesday,  DayOfWeek.Wednesday, DayOfWeek.Thursday };
        var mwf       = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        await EnsureDoctorAsync(doctor1, "BHR-DOC-2021-001", "Senior cardiologist with 12 years of experience in interventional cardiology.",
            weekdays, new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0), ["General Practice", "Cardiology"]);

        await EnsureDoctorAsync(doctor2, "BHR-DOC-2019-002", "General practitioner specialising in paediatric care and family medicine.",
            weekdays, new TimeSpan(9, 0, 0), new TimeSpan(16, 0, 0), ["General Practice", "Pediatrics"]);

        await EnsureDoctorAsync(doctor3, "BHR-DOC-2020-003", "Neurologist with 8 years of experience treating migraines, epilepsy, and stroke rehabilitation.",
            weekdays, new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), ["Neurology", "General Practice"]);

        await EnsureDoctorAsync(doctor4, "BHR-DOC-2022-004", "Ophthalmologist and ENT specialist with expertise in laser eye correction and sinus disorders.",
            sunToThu, new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0), ["Ophthalmology", "ENT"]);

        await EnsureDoctorAsync(doctor5, "BHR-DOC-2018-005", "Psychiatrist with over 10 years in cognitive behavioural therapy and anxiety management.",
            weekdays, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0), ["Psychiatry"]);

        await EnsureDoctorAsync(doctor6, "BHR-DOC-2023-006", "Gynaecologist and dermatologist focused on women's health and skin conditions.",
            mwf, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), ["Gynecology", "Dermatology"]);

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
            var doc1        = await context.Doctors.FirstAsync(d => d.UserId == doctor1!.Id);
            var pat1        = await context.Patients.FirstAsync(p => p.UserId == patient1!.Id);
            var pat2        = await context.Patients.FirstAsync(p => p.UserId == patient2!.Id);
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
