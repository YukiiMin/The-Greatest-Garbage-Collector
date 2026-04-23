using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCitizenReportSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_citizen_reports_users_UserId",
                table: "citizen_reports");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "citizen_reports",
                newName: "citizen_id");

            migrationBuilder.RenameColumn(
                name: "ReportAt",
                table: "citizen_reports",
                newName: "report_at");

            migrationBuilder.RenameColumn(
                name: "CompleteAt",
                table: "citizen_reports",
                newName: "complete_at");

            migrationBuilder.RenameColumn(
                name: "AssignAt",
                table: "citizen_reports",
                newName: "assign_at");

            migrationBuilder.RenameIndex(
                name: "IX_citizen_reports_UserId",
                table: "citizen_reports",
                newName: "IX_citizen_reports_citizen_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "collected_at",
                table: "citizen_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_collecting_at",
                table: "citizen_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_citizen_reports_users_citizen_id",
                table: "citizen_reports",
                column: "citizen_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_citizen_reports_users_citizen_id",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "collected_at",
                table: "citizen_reports");

            migrationBuilder.DropColumn(
                name: "start_collecting_at",
                table: "citizen_reports");

            migrationBuilder.RenameColumn(
                name: "report_at",
                table: "citizen_reports",
                newName: "ReportAt");

            migrationBuilder.RenameColumn(
                name: "complete_at",
                table: "citizen_reports",
                newName: "CompleteAt");

            migrationBuilder.RenameColumn(
                name: "citizen_id",
                table: "citizen_reports",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "assign_at",
                table: "citizen_reports",
                newName: "AssignAt");

            migrationBuilder.RenameIndex(
                name: "IX_citizen_reports_citizen_id",
                table: "citizen_reports",
                newName: "IX_citizen_reports_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_citizen_reports_users_UserId",
                table: "citizen_reports",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
