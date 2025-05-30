using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEGAGame.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerPacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayedPacks",
                columns: table => new
                {
                    PlayedPackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    PackId = table.Column<int>(type: "int", nullable: false),
                    PlayedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayedPacks", x => x.PlayedPackId);
                    table.ForeignKey(
                        name: "FK_PlayedPacks_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayedPacks_QuestionPacks_PackId",
                        column: x => x.PackId,
                        principalTable: "QuestionPacks",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PlayedPacks_PackId",
                table: "PlayedPacks",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayedPacks_PlayerId",
                table: "PlayedPacks",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayedPacks");
        }
    }
}
