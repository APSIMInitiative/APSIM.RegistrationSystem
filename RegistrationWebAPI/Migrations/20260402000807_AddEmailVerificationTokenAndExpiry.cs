using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationTokenAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationSentAtUtc",
                table: "Registrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Registrations",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EmailVerificationToken",
                table: "Registrations",
                column: "EmailVerificationToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registrations_EmailVerificationToken",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "EmailVerificationSentAtUtc",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Registrations");
        }
    }
}
