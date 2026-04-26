using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffCollectorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "team_id",
                table: "staffs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "collector_id",
                table: "staffs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffs_collector_id",
                table: "staffs",
                column: "collector_id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffs_collector_hub_collector_id",
                table: "staffs",
                column: "collector_id",
                principalTable: "collector_hub",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffs_collector_hub_collector_id",
                table: "staffs");

            migrationBuilder.DropIndex(
                name: "IX_staffs_collector_id",
                table: "staffs");

            migrationBuilder.DropColumn(
                name: "collector_id",
                table: "staffs");

            migrationBuilder.AlterColumn<Guid>(
                name: "team_id",
                table: "staffs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
