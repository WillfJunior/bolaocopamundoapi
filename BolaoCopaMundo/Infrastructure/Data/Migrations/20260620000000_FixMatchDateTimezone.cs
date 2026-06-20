using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopaMundo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMatchDateTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE Matches
                  SET MatchDate = DATEADD(HOUR, -3, MatchDate)
                  WHERE Id >= 35 AND Status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE Matches
                  SET MatchDate = DATEADD(HOUR, 3, MatchDate)
                  WHERE Id >= 35 AND Status = 1");
        }
    }
}
