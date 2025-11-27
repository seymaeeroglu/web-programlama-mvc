using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProje.Migrations
{
    /// <inheritdoc />
    public partial class HizmetBransDuzeltme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UzmanlikId",
                table: "Hizmetler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_UzmanlikId",
                table: "Hizmetler",
                column: "UzmanlikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hizmetler_Uzmanliklar_UzmanlikId",
                table: "Hizmetler",
                column: "UzmanlikId",
                principalTable: "Uzmanliklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hizmetler_Uzmanliklar_UzmanlikId",
                table: "Hizmetler");

            migrationBuilder.DropIndex(
                name: "IX_Hizmetler_UzmanlikId",
                table: "Hizmetler");

            migrationBuilder.DropColumn(
                name: "UzmanlikId",
                table: "Hizmetler");
        }
    }
}
