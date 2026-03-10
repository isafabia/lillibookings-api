using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lilliput.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<string>(type: "text", nullable: false),
                    EndTime = table.Column<string>(type: "text", nullable: false),
                    Activity = table.Column<string>(type: "text", nullable: false),
                    KidsCount = table.Column<int>(type: "integer", nullable: false),
                    TeachersCount = table.Column<int>(type: "integer", nullable: false),
                    MedicalNotes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RotaShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<string>(type: "text", nullable: false),
                    EndTime = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Activity = table.Column<string>(type: "text", nullable: true),
                    GroupName = table.Column<string>(type: "text", nullable: true),
                    EmployeeId = table.Column<string>(type: "text", nullable: false),
                    EmployeeName = table.Column<string>(type: "text", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ConfirmedWorked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotaShifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "RotaShifts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
