using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaProfissionaisEVinculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ProfessionalName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Verification = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professionals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "therapeutic_relationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_therapeutic_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_therapeutic_relationships_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_therapeutic_relationships_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professionals_Email",
                table: "professionals",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_therapeutic_relationships_PatientId_ProfessionalId",
                table: "therapeutic_relationships",
                columns: new[] { "PatientId", "ProfessionalId" },
                unique: true,
                filter: "\"Status\" <> 'Ended'");

            migrationBuilder.CreateIndex(
                name: "IX_therapeutic_relationships_ProfessionalId",
                table: "therapeutic_relationships",
                column: "ProfessionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "therapeutic_relationships");

            migrationBuilder.DropTable(
                name: "professionals");
        }
    }
}
