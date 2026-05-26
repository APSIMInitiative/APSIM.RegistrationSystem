using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Emails = table.Column<string>(type: "TEXT", nullable: false),
                    LicenceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", nullable: false),
                    ContactAddress = table.Column<string>(type: "TEXT", nullable: false),
                    LicencePathway = table.Column<int>(type: "INTEGER", nullable: false),
                    AnnualTurnover = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                    EmailVerificationTokenExpiryUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LicenceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                    EmailVerificationTokenExpiryUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Name",
                table: "Organisations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganisationId",
                table: "Users",
                column: "OrganisationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organisations");
        }
    }
}
