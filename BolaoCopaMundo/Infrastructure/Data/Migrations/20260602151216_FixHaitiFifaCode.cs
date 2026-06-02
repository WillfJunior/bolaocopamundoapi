using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopaMundo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixHaitiFifaCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Teams SET FifaCode = 'HTI' WHERE FifaCode = 'HAI'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Teams SET FifaCode = 'HAI' WHERE FifaCode = 'HTI'");
        }
    }
}
