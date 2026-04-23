using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithERD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "gps_lat",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "gps_lng",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "priority_flag",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "route_order",
                table: "citizen_reports");

            migrationBuilder.AddColumn<string>(
                name: "area",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "work_area",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "in_work",
                table: "teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_finish_time",
                table: "teams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_working_time",
                table: "teams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "point_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_delete",
                table: "point_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "assigned_capacity",
                table: "collectors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assign_by",
                table: "citizen_reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deadline",
                table: "citizen_reports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "area",
                table: "users");

            migrationBuilder.DropColumn(
                name: "work_area",
                table: "users");

            migrationBuilder.DropColumn(
                name: "in_work",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "last_finish_time",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "start_working_time",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "point_categories");

            migrationBuilder.DropColumn(
                name: "is_delete",
                table: "point_categories");

            migrationBuilder.DropColumn(
                name: "assigned_capacity",
                table: "collectors");

            migrationBuilder.DropColumn(
                name: "assign_by",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "deadline",
                table: "citizen_reports");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "citizen_reports",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gps_lat",
                table: "citizen_reports",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gps_lng",
                table: "citizen_reports",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "priority_flag",
                table: "citizen_reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "route_order",
                table: "citizen_reports",
                type: "integer",
                nullable: true);
        }
    }
}
