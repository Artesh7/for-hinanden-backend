using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserB = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpRelations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpRelations_TaskId_UserA_UserB",
                table: "HelpRelations",
                columns: new[] { "TaskId", "UserA", "UserB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpRelations_UserA_UserB",
                table: "HelpRelations",
                columns: new[] { "UserA", "UserB" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpRelations");
        }
    }
}
