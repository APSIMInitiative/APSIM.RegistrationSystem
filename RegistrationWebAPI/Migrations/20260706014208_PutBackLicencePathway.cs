using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class PutBackLicencePathway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "licencePathway",
                table: "Organisations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "licencePathway",
                table: "Organisations");
        }
    }
}
