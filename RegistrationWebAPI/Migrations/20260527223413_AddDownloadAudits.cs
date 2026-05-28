using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DownloadType = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAudits_DownloadedAtUtc",
                table: "DownloadAudits",
                column: "DownloadedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAudits_UserEmail",
                table: "DownloadAudits",
                column: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadAudits");
        }
    }
}
