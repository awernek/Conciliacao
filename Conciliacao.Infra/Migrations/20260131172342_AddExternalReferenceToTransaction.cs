using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conciliacao.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReferenceToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
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
