using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaptivePortal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWiFiAccessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessExpiresAt",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AccessGranted",
                table: "PortalUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessGrantedAt",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReconnectionCount",
                table: "PortalUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "PortalUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessExpiresAt",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "AccessGranted",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "AccessGrantedAt",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "ReconnectionCount",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "PortalUsers");
        }
    }
}
