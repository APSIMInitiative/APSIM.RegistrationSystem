using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APSIM.RegistrationAPIV2.Migrations
{
    /// <inheritdoc />
    public partial class InitialRegistrationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistrationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LicenceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganisationName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    OrganisationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OrganisationWebsite = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LicencePathway = table.Column<int>(type: "INTEGER", nullable: true),
                    AnnualTurnover = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ApplicationDate",
                table: "Registrations",
                column: "ApplicationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ContactEmail",
                table: "Registrations",
                column: "ContactEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");
        }
    }
}
