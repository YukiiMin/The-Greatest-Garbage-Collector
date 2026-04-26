using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectorDashboardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // join_team_at may already exist from a previous (rolled-back) migration
            migrationBuilder.Sql(@"
                ALTER TABLE staffs
                ADD COLUMN IF NOT EXISTS join_team_at timestamp with time zone;
            ");

            // team_sessions may already exist from a previous (rolled-back) migration
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS team_sessions (
                    id uuid NOT NULL,
                    team_id uuid NOT NULL,
                    date date NOT NULL,
                    start_at timestamp with time zone NOT NULL,
                    end_at timestamp with time zone,
                    total_reports integer NOT NULL DEFAULT 0,
                    total_capacity numeric(10,2) NOT NULL DEFAULT 0,
                    created_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_team_sessions"" PRIMARY KEY (id),
                    CONSTRAINT ""FK_team_sessions_teams_team_id""
                        FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_team_sessions_team_id"" ON team_sessions (team_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "team_sessions");

            migrationBuilder.DropColumn(
                name: "join_team_at",
                table: "staffs");
        }
    }
}
