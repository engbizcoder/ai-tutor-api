using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ai.Tutor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrgLifecycleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "orgs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisabledAt",
                table: "orgs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "orgs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurgeScheduledAt",
                table: "orgs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetentionDays",
                table: "orgs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "orgs");

            migrationBuilder.DropColumn(
                name: "DisabledAt",
                table: "orgs");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "orgs");

            migrationBuilder.DropColumn(
                name: "PurgeScheduledAt",
                table: "orgs");

            migrationBuilder.DropColumn(
                name: "RetentionDays",
                table: "orgs");
        }
    }
}
