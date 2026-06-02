using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopaMundo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPixKeyAndPendingApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PixKey",
                table: "BolaoGroups",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PixKey",
                table: "BolaoGroups");
        }
    }
}
