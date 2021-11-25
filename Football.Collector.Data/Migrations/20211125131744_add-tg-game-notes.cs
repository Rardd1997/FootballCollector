using Microsoft.EntityFrameworkCore.Migrations;

namespace Football.Collector.Data.Migrations
{
    public partial class addtggamenotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TelegramGames",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TelegramGames");
        }
    }
}
