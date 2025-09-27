using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_OptionTables_And_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DurationOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DurationOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriorityOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DurationOptions_Name",
                table: "DurationOptions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriorityOptions_Name",
                table: "PriorityOptions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DurationOptions");

            migrationBuilder.DropTable(
                name: "PriorityOptions");
        }
    }
}
