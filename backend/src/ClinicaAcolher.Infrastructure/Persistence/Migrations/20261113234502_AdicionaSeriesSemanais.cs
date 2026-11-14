using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaSeriesSemanais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointment_series",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weekday = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LocalStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    GeneratedUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointment_series_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_series_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_series_therapeutic_relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "therapeutic_relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_SeriesId_StartsAt",
                table: "appointments",
                columns: new[] { "SeriesId", "StartsAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointment_series_GeneratedUntil",
                table: "appointment_series",
                column: "GeneratedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_series_PatientId",
                table: "appointment_series",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_series_ProfessionalId",
                table: "appointment_series",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_series_RelationshipId",
                table: "appointment_series",
                column: "RelationshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_appointment_series_SeriesId",
                table: "appointments",
                column: "SeriesId",
                principalTable: "appointment_series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_appointment_series_SeriesId",
                table: "appointments");

            migrationBuilder.DropTable(
                name: "appointment_series");

            migrationBuilder.DropIndex(
                name: "IX_appointments_SeriesId_StartsAt",
                table: "appointments");
        }
    }
}
