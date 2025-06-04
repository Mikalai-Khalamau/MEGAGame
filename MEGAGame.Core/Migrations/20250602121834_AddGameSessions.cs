using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MEGAGame.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Players_HostId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionPlayers_GameSessions_SessionId",
                table: "SessionPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionPlayers_Players_PlayerId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_PlayerId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_SessionId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_HostId",
                table: "GameSessions");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "SessionPlayers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerId",
                table: "SessionPlayers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId1",
                table: "SessionPlayers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "GameSessions",
                keyColumn: "Status",
                keyValue: null,
                column: "Status",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GameSessions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "HostId",
                table: "GameSessions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "GameSessions",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "GameSessions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JoinKey",
                table: "GameSessions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_PlayerId1",
                table: "SessionPlayers",
                column: "PlayerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPlayers_Players_PlayerId1",
                table: "SessionPlayers",
                column: "PlayerId1",
                principalTable: "Players",
                principalColumn: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionPlayers_Players_PlayerId1",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_PlayerId1",
                table: "SessionPlayers");

            migrationBuilder.DropColumn(
                name: "PlayerId1",
                table: "SessionPlayers");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "JoinKey",
                table: "GameSessions");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "SessionPlayers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "SessionPlayers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GameSessions",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "HostId",
                table: "GameSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "GameSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_PlayerId",
                table: "SessionPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_SessionId",
                table: "SessionPlayers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_HostId",
                table: "GameSessions",
                column: "HostId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Players_HostId",
                table: "GameSessions",
                column: "HostId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPlayers_GameSessions_SessionId",
                table: "SessionPlayers",
                column: "SessionId",
                principalTable: "GameSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPlayers_Players_PlayerId",
                table: "SessionPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
