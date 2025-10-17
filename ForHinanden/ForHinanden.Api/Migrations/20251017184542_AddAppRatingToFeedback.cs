using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAppRatingToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Feedback",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "FeedbackText",
                table: "Feedback",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Feedback",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Feedback",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_DeviceId",
                table: "Feedback",
                column: "DeviceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Feedback_DeviceId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Feedback",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "FeedbackText",
                table: "Feedback",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
