using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AnotherLicencePathway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "licencePathway",
                table: "Organisations",
                newName: "LicencePathway");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LicencePathway",
                table: "Organisations",
                newName: "licencePathway");
        }
    }
}
