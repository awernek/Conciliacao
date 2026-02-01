using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conciliacao.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex_ProcessedRequests_IdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "ProcessedRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedRequests_IdempotencyKey",
                table: "ProcessedRequests",
                column: "IdempotencyKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedRequests_IdempotencyKey",
                table: "ProcessedRequests");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "ProcessedRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }


    }
}
