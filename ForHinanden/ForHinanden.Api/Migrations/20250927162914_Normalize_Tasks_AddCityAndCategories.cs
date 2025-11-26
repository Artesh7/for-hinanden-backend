using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class Normalize_Tasks_AddCityAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create lookup tables
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Categories", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Cities", x => x.Id); });

            migrationBuilder.CreateIndex("IX_Categories_Name", "Categories", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_Cities_Name", "Cities", "Name", unique: true);

            // 2) Add CityId (nullable for now)
            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "HelpRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequests_CityId",
                table: "HelpRequests",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpRequests_Cities_CityId",
                table: "HelpRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 3) Create M:M join table
            migrationBuilder.CreateTable(
                name: "TaskCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategories", x => x.Id);
                    table.ForeignKey("FK_TaskCategories_Categories_CategoryId", x => x.CategoryId, "Categories", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_TaskCategories_HelpRequests_TaskId", x => x.TaskId, "HelpRequests", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_TaskCategories_CategoryId", "TaskCategories", "CategoryId");
            migrationBuilder.CreateIndex("IX_TaskCategories_TaskId_CategoryId", "TaskCategories", new[] { "TaskId", "CategoryId" }, unique: true);

            // 4) Drop old columns (no data moves here)
            migrationBuilder.DropColumn(name: "Categories", table: "HelpRequests");
            migrationBuilder.DropColumn(name: "City", table: "HelpRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpRequests_Cities_CityId",
                table: "HelpRequests");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "TaskCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_HelpRequests_CityId",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "HelpRequests");

            migrationBuilder.AddColumn<List<string>>(
                name: "Categories",
                table: "HelpRequests",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "HelpRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
