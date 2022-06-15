using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Football.Collector.Data.Migrations
{
    public partial class preferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TelegramGames");

            migrationBuilder.AddColumn<bool>(
                name: "HasChangingRoom",
                table: "TelegramGames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasParking",
                table: "TelegramGames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasShower",
                table: "TelegramGames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TelegramGames",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasChangingRoom",
                table: "TelegramGames");

            migrationBuilder.DropColumn(
                name: "HasParking",
                table: "TelegramGames");

            migrationBuilder.DropColumn(
                name: "HasShower",
                table: "TelegramGames");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TelegramGames");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TelegramGames",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
