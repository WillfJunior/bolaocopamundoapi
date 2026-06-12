using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopaMundo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Predictions_GroupId",
                table: "Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_MatchId",
                table: "Predictions");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_GroupId_IsProcessed",
                table: "Predictions",
                columns: new[] { "GroupId", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_MatchId_IsProcessed",
                table: "Predictions",
                columns: new[] { "MatchId", "IsProcessed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Predictions_GroupId_IsProcessed",
                table: "Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_MatchId_IsProcessed",
                table: "Predictions");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_GroupId",
                table: "Predictions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_MatchId",
                table: "Predictions",
                column: "MatchId");
        }
    }
}
