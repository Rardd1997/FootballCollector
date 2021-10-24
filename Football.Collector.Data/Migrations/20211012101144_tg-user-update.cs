using Microsoft.EntityFrameworkCore.Migrations;

namespace Football.Collector.Data.Migrations
{
    public partial class tguserupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramUsers_FirstName",
                table: "TelegramUsers");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "TelegramUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "TelegramUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastName",
                table: "TelegramUsers");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "TelegramUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_FirstName",
                table: "TelegramUsers",
                column: "FirstName");
        }
    }
}
