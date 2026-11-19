using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaDisponibilidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "availability_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_blocks", x => x.Id);
                    table.CheckConstraint("ck_availability_blocks_period", "\"EndsAt\" > \"StartsAt\"");
                    table.ForeignKey(
                        name: "FK_availability_blocks_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "professional_availabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefaultSessionMinutes = table.Column<int>(type: "integer", nullable: false),
                    BreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professional_availabilities_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "availability_windows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Weekday = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    AvailabilityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_windows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availability_windows_professional_availabilities_Availabili~",
                        column: x => x.AvailabilityId,
                        principalTable: "professional_availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_availability_blocks_ProfessionalId_StartsAt",
                table: "availability_blocks",
                columns: new[] { "ProfessionalId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_availability_windows_AvailabilityId",
                table: "availability_windows",
                column: "AvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_professional_availabilities_ProfessionalId",
                table: "professional_availabilities",
                column: "ProfessionalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_blocks");

            migrationBuilder.DropTable(
                name: "availability_windows");

            migrationBuilder.DropTable(
                name: "professional_availabilities");
        }
    }
}
