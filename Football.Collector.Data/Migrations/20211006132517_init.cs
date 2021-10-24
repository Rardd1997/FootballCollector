using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Football.Collector.Data.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValueSql: "newid()"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValueSql: "newid()"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInMins = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValueSql: "newid()"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TelegramId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramChatUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    TelegramChatId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelegramUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChatUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramChatUsers_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramGamePlayers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValueSql: "newid()"),
                    TelegramGameId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TelegramUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramGamePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramGamePlayers_TelegramGames_TelegramGameId",
                        column: x => x.TelegramGameId,
                        principalTable: "TelegramGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramGamePlayers_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceUsers_Username",
                table: "ServiceUsers",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatUsers_TelegramUserId",
                table: "TelegramChatUsers",
                column: "TelegramUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramGamePlayers_TelegramGameId_TelegramUserId",
                table: "TelegramGamePlayers",
                columns: new[] { "TelegramGameId", "TelegramUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramGamePlayers_TelegramUserId",
                table: "TelegramGamePlayers",
                column: "TelegramUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_FirstName",
                table: "TelegramUsers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_TelegramId",
                table: "TelegramUsers",
                column: "TelegramId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceUsers");

            migrationBuilder.DropTable(
                name: "TelegramChatUsers");

            migrationBuilder.DropTable(
                name: "TelegramGamePlayers");

            migrationBuilder.DropTable(
                name: "TelegramGames");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
