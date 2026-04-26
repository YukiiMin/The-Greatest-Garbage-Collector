using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkAreasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "area",
                table: "users");

            migrationBuilder.DropColumn(
                name: "work_area",
                table: "users");

            migrationBuilder.DropColumn(
                name: "work_area",
                table: "enterprise_hub");

            migrationBuilder.DropColumn(
                name: "work_area",
                table: "collector_hub");

            migrationBuilder.AddColumn<Guid>(
                name: "work_area_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "work_area_id",
                table: "enterprise_hub",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "work_area_id",
                table: "collector_hub",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "work_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_areas", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_areas_work_areas_parent_id",
                        column: x => x.parent_id,
                        principalTable: "work_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_work_area_id",
                table: "users",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_work_area_id",
                table: "teams",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_hub_work_area_id",
                table: "enterprise_hub",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_collector_hub_work_area_id",
                table: "collector_hub",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_areas_parent_id",
                table: "work_areas",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_collector_hub_work_areas_work_area_id",
                table: "collector_hub",
                column: "work_area_id",
                principalTable: "work_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_enterprise_hub_work_areas_work_area_id",
                table: "enterprise_hub",
                column: "work_area_id",
                principalTable: "work_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_work_areas_work_area_id",
                table: "teams",
                column: "work_area_id",
                principalTable: "work_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_users_work_areas_work_area_id",
                table: "users",
                column: "work_area_id",
                principalTable: "work_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_collector_hub_work_areas_work_area_id",
                table: "collector_hub");

            migrationBuilder.DropForeignKey(
                name: "FK_enterprise_hub_work_areas_work_area_id",
                table: "enterprise_hub");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_work_areas_work_area_id",
                table: "teams");

            migrationBuilder.DropForeignKey(
                name: "FK_users_work_areas_work_area_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "work_areas");

            migrationBuilder.DropIndex(
                name: "IX_users_work_area_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_teams_work_area_id",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_enterprise_hub_work_area_id",
                table: "enterprise_hub");

            migrationBuilder.DropIndex(
                name: "IX_collector_hub_work_area_id",
                table: "collector_hub");

            migrationBuilder.DropColumn(
                name: "work_area_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "work_area_id",
                table: "enterprise_hub");

            migrationBuilder.DropColumn(
                name: "work_area_id",
                table: "collector_hub");

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

            migrationBuilder.AddColumn<string>(
                name: "work_area",
                table: "enterprise_hub",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "work_area",
                table: "collector_hub",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
