using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToLookupOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpRequests_Cities_CityId",
                table: "HelpRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_HelpRequests_TaskId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskCategories_HelpRequests_TaskId",
                table: "TaskCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskOffers_HelpRequests_TaskId",
                table: "TaskOffers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HelpRequests",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "HelpRequests");

            migrationBuilder.RenameTable(
                name: "HelpRequests",
                newName: "Tasks");

            migrationBuilder.RenameIndex(
                name: "IX_HelpRequests_CityId",
                table: "Tasks",
                newName: "IX_Tasks_CityId");

            migrationBuilder.AddColumn<Guid>(
                name: "DurationOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PriorityOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DurationOptionId",
                table: "Tasks",
                column: "DurationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Tasks_TaskId",
                table: "Messages",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCategories_Tasks_TaskId",
                table: "TaskCategories",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskOffers_Tasks_TaskId",
                table: "TaskOffers",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Cities_CityId",
                table: "Tasks",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_DurationOptions_DurationOptionId",
                table: "Tasks",
                column: "DurationOptionId",
                principalTable: "DurationOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_PriorityOptions_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId",
                principalTable: "PriorityOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Tasks_TaskId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskCategories_Tasks_TaskId",
                table: "TaskCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskOffers_Tasks_TaskId",
                table: "TaskOffers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Cities_CityId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_DurationOptions_DurationOptionId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_PriorityOptions_PriorityOptionId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DurationOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DurationOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PriorityOptionId",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "HelpRequests");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_CityId",
                table: "HelpRequests",
                newName: "IX_HelpRequests_CityId");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "HelpRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "HelpRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HelpRequests",
                table: "HelpRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpRequests_Cities_CityId",
                table: "HelpRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_HelpRequests_TaskId",
                table: "Messages",
                column: "TaskId",
                principalTable: "HelpRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCategories_HelpRequests_TaskId",
                table: "TaskCategories",
                column: "TaskId",
                principalTable: "HelpRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskOffers_HelpRequests_TaskId",
                table: "TaskOffers",
                column: "TaskId",
                principalTable: "HelpRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
