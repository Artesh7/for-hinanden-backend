using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Offers_ReturnAndOwnerQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_UserProfiles_UserProfileId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserProfileId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "Ratings",
                newName: "TaskId");

            migrationBuilder.AddColumn<string>(
                name: "ToUserId",
                table: "Ratings",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HelpRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "HelpRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferedBy = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskOffers_HelpRequests_TaskId",
                        column: x => x.TaskId,
                        principalTable: "HelpRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskOffers_TaskId_OfferedBy",
                table: "TaskOffers",
                columns: new[] { "TaskId", "OfferedBy" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskOffers");

            migrationBuilder.DropColumn(
                name: "ToUserId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "Categories",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "HelpRequests");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Ratings",
                newName: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserProfileId",
                table: "Ratings",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_UserProfiles_UserProfileId",
                table: "Ratings",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
