using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProje.Migrations
{
    /// <inheritdoc />
    public partial class HizmetAdiKaldirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ad",
                table: "Hizmetler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "Hizmetler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
