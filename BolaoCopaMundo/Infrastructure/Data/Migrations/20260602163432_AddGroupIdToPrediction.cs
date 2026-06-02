using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopaMundo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToPrediction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Palpites antigos não têm grupo — são dados de teste, podem ser removidos
            migrationBuilder.Sql("DELETE FROM Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_UserId_MatchId",
                table: "Predictions");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Predictions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_GroupId",
                table: "Predictions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId_MatchId_GroupId",
                table: "Predictions",
                columns: new[] { "UserId", "MatchId", "GroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Predictions_BolaoGroups_GroupId",
                table: "Predictions",
                column: "GroupId",
                principalTable: "BolaoGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predictions_BolaoGroups_GroupId",
                table: "Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_GroupId",
                table: "Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_UserId_MatchId_GroupId",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Predictions");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId_MatchId",
                table: "Predictions",
                columns: new[] { "UserId", "MatchId" },
                unique: true);
        }
    }
}
