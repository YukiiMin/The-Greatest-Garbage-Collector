using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SchemaRefactor20260427 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_collector_hubs_collectors_collector_id",
                table: "collector_hubs");

            migrationBuilder.DropForeignKey(
                name: "FK_enterprise_staffs_teams_team_id",
                table: "enterprise_staffs");

            migrationBuilder.DropTable(
                name: "user_points");

            migrationBuilder.DropIndex(
                name: "IX_enterprise_staffs_team_id",
                table: "enterprise_staffs");

            migrationBuilder.DropIndex(
                name: "IX_collector_hubs_collector_id",
                table: "collector_hubs");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "enterprise_staffs");

            migrationBuilder.DropColumn(
                name: "collector_id",
                table: "collector_hubs");

            migrationBuilder.RenameColumn(
                name: "join_team_at",
                table: "enterprise_staffs",
                newName: "join_hub_at");

            migrationBuilder.AddColumn<int>(
                name: "total_points",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "join_hub_at",
                table: "enterprise_hub",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "point_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_id = table.Column<Guid>(type: "uuid", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_point_transactions_citizen_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "citizen_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_point_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_report_id",
                table: "point_transactions",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_user_id",
                table: "point_transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "point_transactions");

            migrationBuilder.DropColumn(
                name: "total_points",
                table: "users");

            migrationBuilder.DropColumn(
                name: "join_hub_at",
                table: "enterprise_hub");

            migrationBuilder.RenameColumn(
                name: "join_hub_at",
                table: "enterprise_staffs",
                newName: "join_team_at");

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "enterprise_staffs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "collector_id",
                table: "collector_hubs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "user_points",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leaderboard_opt_out = table.Column<bool>(type: "boolean", nullable: false),
                    month_points = table.Column<int>(type: "integer", nullable: false),
                    total_points = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    week_points = table.Column<int>(type: "integer", nullable: false),
                    work_area_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    year_points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_points", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_points_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_staffs_team_id",
                table: "enterprise_staffs",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_collector_hubs_collector_id",
                table: "collector_hubs",
                column: "collector_id");

            migrationBuilder.AddForeignKey(
                name: "FK_collector_hubs_collectors_collector_id",
                table: "collector_hubs",
                column: "collector_id",
                principalTable: "collectors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_enterprise_staffs_teams_team_id",
                table: "enterprise_staffs",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
