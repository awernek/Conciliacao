using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conciliacao.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReferenceToTransaction2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona a coluna só se ainda não existir (idempotente).
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Transactions') AND name = 'ExternalReference'
)
BEGIN
    ALTER TABLE [Transactions] ADD [ExternalReference] nvarchar(max) NOT NULL DEFAULT '';
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "Transactions");
        }
    }
}
