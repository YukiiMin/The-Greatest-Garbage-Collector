using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantCitizenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CitizenId column was never applied to DB (previous migration was rolled back)
            // Nothing to do here — snapshot sync only
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CitizenId",
                table: "citizen_reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_citizen_reports_CitizenId",
                table: "citizen_reports",
                column: "CitizenId");

            migrationBuilder.AddForeignKey(
                name: "FK_citizen_reports_users_CitizenId",
                table: "citizen_reports",
                column: "CitizenId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
