using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameHubTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collectors_enterprise_id",
                table: "collectors");

            migrationBuilder.DropForeignKey(
                name: "FK_point_categories_enterprises_enterprise_id",
                table: "point_categories");

            migrationBuilder.DropForeignKey(
                name: "FK_staffs_enterprises_enterprise_id",
                table: "staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_collectors_collector_id",
                table: "teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_enterprises",
                table: "enterprises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_collectors",
                table: "collectors");

            migrationBuilder.RenameTable(
                name: "enterprises",
                newName: "enterprise_hub");

            migrationBuilder.RenameTable(
                name: "collectors",
                newName: "collector_hub");

            migrationBuilder.RenameIndex(
                name: "IX_enterprises_email",
                table: "enterprise_hub",
                newName: "IX_enterprise_hub_email");

            migrationBuilder.RenameIndex(
                name: "IX_collectors_enterprise_id",
                table: "collector_hub",
                newName: "IX_collector_hub_enterprise_id");

            migrationBuilder.RenameIndex(
                name: "IX_collectors_email",
                table: "collector_hub",
                newName: "IX_collector_hub_email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_enterprise_hub",
                table: "enterprise_hub",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_collector_hub",
                table: "collector_hub",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_collector_hub_enterprise_id",
                table: "collector_hub",
                column: "enterprise_id",
                principalTable: "enterprise_hub",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_point_categories_enterprise_hub_enterprise_id",
                table: "point_categories",
                column: "enterprise_id",
                principalTable: "enterprise_hub",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_staffs_enterprise_hub_enterprise_id",
                table: "staffs",
                column: "enterprise_id",
                principalTable: "enterprise_hub",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_collector_hub_collector_id",
                table: "teams",
                column: "collector_id",
                principalTable: "collector_hub",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collector_hub_enterprise_id",
                table: "collector_hub");

            migrationBuilder.DropForeignKey(
                name: "FK_point_categories_enterprise_hub_enterprise_id",
                table: "point_categories");

            migrationBuilder.DropForeignKey(
                name: "FK_staffs_enterprise_hub_enterprise_id",
                table: "staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_collector_hub_collector_id",
                table: "teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_enterprise_hub",
                table: "enterprise_hub");

            migrationBuilder.DropPrimaryKey(
                name: "PK_collector_hub",
                table: "collector_hub");

            migrationBuilder.RenameTable(
                name: "enterprise_hub",
                newName: "enterprises");

            migrationBuilder.RenameTable(
                name: "collector_hub",
                newName: "collectors");

            migrationBuilder.RenameIndex(
                name: "IX_enterprise_hub_email",
                table: "enterprises",
                newName: "IX_enterprises_email");

            migrationBuilder.RenameIndex(
                name: "IX_collector_hub_enterprise_id",
                table: "collectors",
                newName: "IX_collectors_enterprise_id");

            migrationBuilder.RenameIndex(
                name: "IX_collector_hub_email",
                table: "collectors",
                newName: "IX_collectors_email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_enterprises",
                table: "enterprises",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_collectors",
                table: "collectors",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_collectors_enterprise_id",
                table: "collectors",
                column: "enterprise_id",
                principalTable: "enterprises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_point_categories_enterprises_enterprise_id",
                table: "point_categories",
                column: "enterprise_id",
                principalTable: "enterprises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_staffs_enterprises_enterprise_id",
                table: "staffs",
                column: "enterprise_id",
                principalTable: "enterprises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_collectors_collector_id",
                table: "teams",
                column: "collector_id",
                principalTable: "collectors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
