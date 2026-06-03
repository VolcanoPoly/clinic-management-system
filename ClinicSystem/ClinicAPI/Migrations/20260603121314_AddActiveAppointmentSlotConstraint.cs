using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveAppointmentSlotConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Doctor_DateTime_Active",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDateTime" },
                unique: true,
                filter: "[Status] <> 5 AND [Status] <> 6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_Doctor_DateTime_Active",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");
        }
    }
}
