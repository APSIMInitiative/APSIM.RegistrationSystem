using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberOrganisationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberOrganisations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganisationName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    OrganisationDomain = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    MembershipEstablishmentDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberOrganisations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberOrganisationRegistration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", nullable: false),
                    LicenceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AgreesToTerms = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemberOrganisationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberOrganisationRegistration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberOrganisationRegistration_MemberOrganisations_MemberOrganisationId",
                        column: x => x.MemberOrganisationId,
                        principalTable: "MemberOrganisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberOrganisationRegistration_MemberOrganisationId",
                table: "MemberOrganisationRegistration",
                column: "MemberOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOrganisations_OrganisationDomain",
                table: "MemberOrganisations",
                column: "OrganisationDomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberOrganisationRegistration");

            migrationBuilder.DropTable(
                name: "MemberOrganisations");
        }
    }
}
