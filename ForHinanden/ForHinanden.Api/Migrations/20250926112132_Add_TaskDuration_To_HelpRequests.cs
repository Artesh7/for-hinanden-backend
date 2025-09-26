using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_TaskDuration_To_HelpRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "HelpRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "HelpRequests");
        }
    }
}
