using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrganisationApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualTurnover",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ContactAddress",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LicencePathway",
                table: "Organisations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnualTurnover",
                table: "Organisations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContactAddress",
                table: "Organisations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Organisations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Organisations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Organisations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LicencePathway",
                table: "Organisations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
