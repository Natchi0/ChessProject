using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class addMathedWithId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MatchedWith",
                table: "MatchRequests",
                newName: "MatchedPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRequests_MatchedPlayerId",
                table: "MatchRequests",
                column: "MatchedPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchRequests_Players_MatchedPlayerId",
                table: "MatchRequests",
                column: "MatchedPlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchRequests_Players_MatchedPlayerId",
                table: "MatchRequests");

            migrationBuilder.DropIndex(
                name: "IX_MatchRequests_MatchedPlayerId",
                table: "MatchRequests");

            migrationBuilder.RenameColumn(
                name: "MatchedPlayerId",
                table: "MatchRequests",
                newName: "MatchedWith");
        }
    }
}
