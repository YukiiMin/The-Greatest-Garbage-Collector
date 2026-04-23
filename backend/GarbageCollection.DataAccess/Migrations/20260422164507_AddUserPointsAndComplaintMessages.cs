using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPointsAndComplaintMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "messages",
                table: "complaints",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateTable(
                name: "password_otp",
                columns: table => new
                {
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    otp_code = table.Column<string>(type: "character varying(72)", maxLength: 72, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_otp", x => x.email);
                    table.ForeignKey(
                        name: "FK_password_otp_users_email",
                        column: x => x.email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_points",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_points = table.Column<int>(type: "integer", nullable: false),
                    month_points = table.Column<int>(type: "integer", nullable: false),
                    year_points = table.Column<int>(type: "integer", nullable: false),
                    total_points = table.Column<int>(type: "integer", nullable: false),
                    leaderboard_opt_out = table.Column<bool>(type: "boolean", nullable: false),
                    work_area_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_otp");

            migrationBuilder.DropTable(
                name: "user_points");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_users_email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "messages",
                table: "complaints");
        }
    }
}
