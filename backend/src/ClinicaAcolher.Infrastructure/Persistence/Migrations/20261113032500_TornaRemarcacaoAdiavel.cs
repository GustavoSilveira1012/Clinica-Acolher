using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaAcolher.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TornaRemarcacaoAdiavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RN-CAN-03: na remarcação, a sessão antiga aponta para a nova antes de ela existir
            migrationBuilder.Sql("""
                ALTER TABLE appointments
                ALTER CONSTRAINT "FK_appointments_appointments_RescheduledToId"
                DEFERRABLE INITIALLY DEFERRED;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE appointments
                ALTER CONSTRAINT "FK_appointments_appointments_RescheduledToId"
                NOT DEFERRABLE;
                """);
        }
    }
}
