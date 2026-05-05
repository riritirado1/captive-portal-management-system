using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaptivePortal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PBI7_CampaignAndAdvancedAds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "Advertisements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Advertisements",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageFileSize",
                table: "Advertisements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSize",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDisplayed",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Advertisements",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Advertisements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Advertisements",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Advertisements",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Advertisements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Budget = table.Column<decimal>(type: "TEXT", nullable: true),
                    TargetAudience = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_CampaignId",
                table: "Advertisements",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_CreatedAt",
                table: "Advertisements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_Status",
                table: "Advertisements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedAt",
                table: "Campaigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsActive",
                table: "Campaigns",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Campaigns_CampaignId",
                table: "Advertisements",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Campaigns_CampaignId",
                table: "Advertisements");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_CampaignId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_CreatedAt",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_Status",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ImageFileSize",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ImageSize",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "LastDisplayed",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Advertisements");
        }
    }
}
