using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPoliticaDeFaltas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attendance_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    MinimumNoticeHours = table.Column<int>(type: "integer", nullable: false),
                    LateCancellationChargePercent = table.Column<int>(type: "integer", nullable: false),
                    NoShowChargePercent = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_policies", x => x.Id);
                    table.CheckConstraint("ck_attendance_policies_late_percent", "\"LateCancellationChargePercent\" BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_attendance_policies_no_show_percent", "\"NoShowChargePercent\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_attendance_policies_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attendance_policy_acceptances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersion = table.Column<int>(type: "integer", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_policy_acceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_policy_acceptances_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_policy_acceptances_attendance_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "attendance_policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_policy_acceptances_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_policies_ProfessionalId_Version",
                table: "attendance_policies",
                columns: new[] { "ProfessionalId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_policy_acceptances_PatientId_PolicyId",
                table: "attendance_policy_acceptances",
                columns: new[] { "PatientId", "PolicyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_policy_acceptances_PolicyId",
                table: "attendance_policy_acceptances",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_policy_acceptances_ProfessionalId_PatientId",
                table: "attendance_policy_acceptances",
                columns: new[] { "ProfessionalId", "PatientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_policy_acceptances");

            migrationBuilder.DropTable(
                name: "attendance_policies");
        }
    }
}
