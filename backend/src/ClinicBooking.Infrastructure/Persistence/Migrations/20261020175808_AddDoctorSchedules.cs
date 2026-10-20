using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentDurationMinutes",
                table: "Doctors",
                type: "int",
                nullable: false,
                // Existing doctors get the domain default; 0 would mean zero-minute appointments.
                defaultValue: 30);

            migrationBuilder.CreateTable(
                name: "DoctorScheduleBlocks",
                columns: table => new
                {
                    Day = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Start = table.Column<TimeOnly>(type: "time", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    End = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorScheduleBlocks", x => new { x.DoctorId, x.Day, x.Start });
                    table.ForeignKey(
                        name: "FK_DoctorScheduleBlocks_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorScheduleBlocks");

            migrationBuilder.DropColumn(
                name: "AppointmentDurationMinutes",
                table: "Doctors");
        }
    }
}
