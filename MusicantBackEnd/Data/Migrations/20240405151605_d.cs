using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicantBackEnd.Data.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discription",
                table: "Communities");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Communities");

            migrationBuilder.AddColumn<string>(
                name: "Discription",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
