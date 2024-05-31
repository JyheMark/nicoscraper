using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quitmed_scraper.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedDispensaryToExecutionLogEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DispensaryId",
                table: "ExecutionLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_DispensaryId",
                table: "ExecutionLogs",
                column: "DispensaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExecutionLogs_Dispensaries_DispensaryId",
                table: "ExecutionLogs",
                column: "DispensaryId",
                principalTable: "Dispensaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExecutionLogs_Dispensaries_DispensaryId",
                table: "ExecutionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExecutionLogs_DispensaryId",
                table: "ExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DispensaryId",
                table: "ExecutionLogs");
        }
    }
}
