using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StThomasMission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFamily3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EparchyIndia",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "ParishIndia",
                table: "FamilyMembers");

            migrationBuilder.AddColumn<string>(
                name: "EparchyIndia",
                table: "Families",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParishIndia",
                table: "Families",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EparchyIndia",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "ParishIndia",
                table: "Families");

            migrationBuilder.AddColumn<string>(
                name: "EparchyIndia",
                table: "FamilyMembers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParishIndia",
                table: "FamilyMembers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
