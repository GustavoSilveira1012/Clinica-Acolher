using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaSessoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RescheduledToId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.CheckConstraint("ck_appointments_period", "\"EndsAt\" > \"StartsAt\"");
                    table.ForeignKey(
                        name: "FK_appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_appointments_RescheduledToId",
                        column: x => x.RescheduledToId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_therapeutic_relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "therapeutic_relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId",
                table: "appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ProfessionalId_StartsAt",
                table: "appointments",
                columns: new[] { "ProfessionalId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_RelationshipId",
                table: "appointments",
                column: "RelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_RescheduledToId",
                table: "appointments",
                column: "RescheduledToId");

            // RN-AGD-03 e RN-AGD-04: sessões ativas nunca se sobrepõem, nem por profissional, nem por paciente
            migrationBuilder.Sql("""
                ALTER TABLE appointments
                ADD CONSTRAINT ex_appointments_professional_overlap
                EXCLUDE USING gist (
                    "ProfessionalId" WITH =,
                    tstzrange("StartsAt", "EndsAt", '[)') WITH &&
                ) WHERE ("Status" IN ('Scheduled', 'Confirmed'));
                """);

            migrationBuilder.Sql("""
                ALTER TABLE appointments
                ADD CONSTRAINT ex_appointments_patient_overlap
                EXCLUDE USING gist (
                    "PatientId" WITH =,
                    tstzrange("StartsAt", "EndsAt", '[)') WITH &&
                ) WHERE ("Status" IN ('Scheduled', 'Confirmed'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");
        }
    }
}
