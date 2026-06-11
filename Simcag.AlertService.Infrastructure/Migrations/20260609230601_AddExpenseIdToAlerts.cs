using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcag.AlertService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseIdToAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseId",
                table: "Alerts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ExpenseId",
                table: "Alerts",
                column: "ExpenseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Alerts_ExpenseId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Alerts"" DROP COLUMN IF EXISTS ""ExpenseId"";");
        }
    }
}
