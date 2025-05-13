using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StThomasMission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaptismalName",
                table: "FamilyMembers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBaptism",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfChrismation",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfDeath",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfHolyCommunion",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfMarriage",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Families",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Families",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GiftAid",
                table: "Families",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "Families",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostCode",
                table: "Families",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetName",
                table: "Families",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaptismalName",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "DateOfBaptism",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "DateOfChrismation",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "DateOfDeath",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "DateOfHolyCommunion",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "DateOfMarriage",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "EparchyIndia",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "ParishIndia",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "GiftAid",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "StreetName",
                table: "Families");
        }
    }
}
