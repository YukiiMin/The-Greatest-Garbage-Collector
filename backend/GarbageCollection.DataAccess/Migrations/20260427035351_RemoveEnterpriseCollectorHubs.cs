using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEnterpriseCollectorHubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_enterprise_staffs_enterprise_collector_hubs_enterprise_hub_~",
                table: "enterprise_staffs");

            migrationBuilder.DropTable(
                name: "enterprise_collector_hubs");

            migrationBuilder.DropIndex(
                name: "IX_enterprise_staffs_enterprise_hub_id",
                table: "enterprise_staffs");

            migrationBuilder.DropColumn(
                name: "enterprise_hub_id",
                table: "enterprise_staffs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "enterprise_hub_id",
                table: "enterprise_staffs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "enterprise_collector_hubs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enterprise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    assigned_capacity = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enterprise_collector_hubs", x => x.id);
                    table.ForeignKey(
                        name: "FK_enterprise_collector_hubs_enterprise_hub_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "enterprise_hub",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enterprise_collector_hubs_work_areas_work_area_id",
                        column: x => x.work_area_id,
                        principalTable: "work_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_staffs_enterprise_hub_id",
                table: "enterprise_staffs",
                column: "enterprise_hub_id");

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_collector_hubs_email",
                table: "enterprise_collector_hubs",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_collector_hubs_enterprise_id",
                table: "enterprise_collector_hubs",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_collector_hubs_work_area_id",
                table: "enterprise_collector_hubs",
                column: "work_area_id");

            migrationBuilder.AddForeignKey(
                name: "FK_enterprise_staffs_enterprise_collector_hubs_enterprise_hub_~",
                table: "enterprise_staffs",
                column: "enterprise_hub_id",
                principalTable: "enterprise_collector_hubs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
