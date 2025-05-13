using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StThomasMission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFamily2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviousFamilyId",
                table: "Families",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Families_PreviousFamilyId",
                table: "Families",
                column: "PreviousFamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Families_PreviousFamilyId",
                table: "Families",
                column: "PreviousFamilyId",
                principalTable: "Families",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_Families_PreviousFamilyId",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Families_PreviousFamilyId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "PreviousFamilyId",
                table: "Families");
        }
    }
}
