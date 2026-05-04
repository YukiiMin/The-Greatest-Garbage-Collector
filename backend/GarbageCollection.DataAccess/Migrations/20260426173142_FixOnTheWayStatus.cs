using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixOnTheWayStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đổi tất cả records có status 'OnTheWay' → 'Processing'
            migrationBuilder.Sql(@"
                UPDATE citizen_reports
                SET ""Status"" = 'Processing', ""UpdatedAt"" = NOW()
                WHERE ""Status"" = 'OnTheWay';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE citizen_reports
                SET ""Status"" = 'OnTheWay', ""UpdatedAt"" = NOW()
                WHERE ""Status"" = 'Processing';
            ");
        }
    }
}
