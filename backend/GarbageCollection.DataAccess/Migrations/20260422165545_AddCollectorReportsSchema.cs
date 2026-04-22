using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectorReportsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "citizen_reports",
                newName: "team_id");

            migrationBuilder.AddColumn<string>(
                name: "dispatch_time",
                table: "teams",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "route_optimized",
                table: "teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "work_area_id",
                table: "teams",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "collector_hubs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enterprise_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    lat = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    lng = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    work_area_ids = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collector_hubs", x => x.id);
                    table.ForeignKey(
                        name: "FK_collector_hubs_enterprises_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "enterprises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_collector_hubs_enterprise_id",
                table: "collector_hubs",
                column: "enterprise_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collector_hubs");

            migrationBuilder.DropColumn(
                name: "dispatch_time",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "route_optimized",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "work_area_id",
                table: "teams");

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

            migrationBuilder.RenameColumn(
                name: "team_id",
                table: "citizen_reports",
                newName: "TeamId");
        }
    }
}
