﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quitmed_scraper.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispensaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ScrapeUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispensaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DispensaryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutionLogs_Dispensaries_DispensaryId",
                        column: x => x.DispensaryId,
                        principalTable: "Dispensaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Vendor = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    InStock = table.Column<bool>(type: "boolean", nullable: false),
                    DispensaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Dispensaries_DispensaryId",
                        column: x => x.DispensaryId,
                        principalTable: "Dispensaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DispensaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSummaries_Dispensaries_DispensaryId",
                        column: x => x.DispensaryId,
                        principalTable: "Dispensaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSummaries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalPricing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalPricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalPricing_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dispensaries",
                columns: new[] { "Id", "Name", "ScrapeUrl" },
                values: new object[] { new Guid("c123f55e-9d6b-4dd4-9754-11cddd50ef62"), "QuitMed", "https://quitmed.com.au/collections/all" });

            migrationBuilder.CreateIndex(
                name: "IX_Dispensaries_Name",
                table: "Dispensaries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispensaries_ScrapeUrl",
                table: "Dispensaries",
                column: "ScrapeUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventSummaries_DispensaryId",
                table: "EventSummaries",
                column: "DispensaryId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSummaries_ProductId",
                table: "EventSummaries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_DispensaryId",
                table: "ExecutionLogs",
                column: "DispensaryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPricing_ProductId",
                table: "HistoricalPricing",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DispensaryId",
                table: "Products",
                column: "DispensaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Key",
                table: "Products",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSummaries");

            migrationBuilder.DropTable(
                name: "ExecutionLogs");

            migrationBuilder.DropTable(
                name: "HistoricalPricing");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Dispensaries");
        }
    }
}
