using ClinicAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<DoctorLeave> DoctorLeaves { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Many-to-Many: Doctor <-> Specialization
            builder.Entity<DoctorSpecialization>()
                .HasKey(ds => new { ds.DoctorId, ds.SpecializationId });

            builder.Entity<DoctorSpecialization>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.Specializations)
                .HasForeignKey(ds => ds.DoctorId);

            builder.Entity<DoctorSpecialization>()
                .HasOne(ds => ds.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(ds => ds.SpecializationId);

            // Configure One-to-One: Appointment <-> VisitRecord
            builder.Entity<VisitRecord>()
                .HasOne(v => v.Appointment)
                .WithOne(a => a.VisitRecord)
                .HasForeignKey<VisitRecord>(v => v.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-One: VisitRecord <-> Prescription
            builder.Entity<Prescription>()
                .HasOne(p => p.VisitRecord)
                .WithOne(v => v.Prescription)
                .HasForeignKey<Prescription>(p => p.VisitRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.AppointmentDateTime })
                .IsUnique()
                .HasFilter("[Status] <> 5 AND [Status] <> 6")
                .HasDatabaseName("IX_Appointments_Doctor_DateTime_Active");

            // Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Patient", NormalizedName = "PATIENT" },
                new IdentityRole { Id = "2", Name = "Doctor", NormalizedName = "DOCTOR" },
                new IdentityRole { Id = "3", Name = "Receptionist", NormalizedName = "RECEPTIONIST" },
                new IdentityRole { Id = "4", Name = "ClinicManager", NormalizedName = "CLINICMANAGER" }
            );
        }
    }
}
