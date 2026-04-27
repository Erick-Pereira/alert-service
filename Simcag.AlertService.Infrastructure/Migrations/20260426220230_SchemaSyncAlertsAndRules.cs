using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcag.AlertService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaSyncAlertsAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlertCategory",
                table: "Alerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalyzedAt",
                table: "Alerts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Alerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "Alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeviationPercentage",
                table: "Alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Alerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Resolved",
                table: "Alerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "Alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupplierId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinDataPoints = table.Column<int>(type: "integer", nullable: false),
                    EvaluationWindow = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DefaultSeverity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Resolved",
                table: "Alerts",
                column: "Resolved");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_Category",
                table: "AlertRules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_IsEnabled",
                table: "AlertRules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_Type",
                table: "AlertRules",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Resolved",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AlertCategory",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AnalyzedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DeviationPercentage",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Resolved",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Alerts");
        }
    }
}
