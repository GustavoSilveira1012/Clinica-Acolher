using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTrilhaDeAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_entries",
                table: "audit_entries");

            migrationBuilder.RenameTable(
                name: "audit_entries",
                newName: "audit_log");

            migrationBuilder.RenameIndex(
                name: "IX_audit_entries_PatientId",
                table: "audit_log",
                newName: "IX_audit_log_PatientId");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceType",
                table: "audit_log",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "audit_log",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log",
                column: "Sequence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log");

            migrationBuilder.RenameTable(
                name: "audit_log",
                newName: "audit_entries");

            migrationBuilder.RenameIndex(
                name: "IX_audit_log_PatientId",
                table: "audit_entries",
                newName: "IX_audit_entries_PatientId");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceType",
                table: "audit_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "audit_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_entries",
                table: "audit_entries",
                column: "Sequence");
        }
    }
}
