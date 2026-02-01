using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conciliacao.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Fix_IdempotencyKey_Length_And_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedRequests_IdempotencyKey",
                table: "ProcessedRequests");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "ProcessedRequests",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedRequests_IdempotencyKey",
                table: "ProcessedRequests",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
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

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "ProcessedRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
