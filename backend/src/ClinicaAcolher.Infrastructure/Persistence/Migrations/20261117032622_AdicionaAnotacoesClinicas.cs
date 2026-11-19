using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAnotacoesClinicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clinical_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_notes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clinical_notes_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clinical_notes_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clinical_notes_therapeutic_relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "therapeutic_relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clinical_note_addenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    EncryptedContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_note_addenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_note_addenda_clinical_notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "clinical_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clinical_note_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    EncryptedContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    SavedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_note_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_note_revisions_clinical_notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "clinical_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_note_addenda_NoteId_Number",
                table: "clinical_note_addenda",
                columns: new[] { "NoteId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clinical_note_revisions_NoteId_Number",
                table: "clinical_note_revisions",
                columns: new[] { "NoteId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clinical_notes_AppointmentId",
                table: "clinical_notes",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_clinical_notes_PatientId",
                table: "clinical_notes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_clinical_notes_ProfessionalId_PatientId",
                table: "clinical_notes",
                columns: new[] { "ProfessionalId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_notes_RelationshipId",
                table: "clinical_notes",
                column: "RelationshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_note_addenda");

            migrationBuilder.DropTable(
                name: "clinical_note_revisions");

            migrationBuilder.DropTable(
                name: "clinical_notes");
        }
    }
}
