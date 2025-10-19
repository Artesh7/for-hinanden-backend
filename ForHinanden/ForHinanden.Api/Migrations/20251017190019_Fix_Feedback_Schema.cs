using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Feedback_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "Feedback");

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

            // 1) Rating med default 5 (ikke 0)
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Feedback",
                type: "integer",
                nullable: false,
                defaultValue: 5
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Feedback",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feedback",
                table: "Feedback",
                column: "Id");

            // (valgfrit men godt): sæt CreatedAt default i DB for nye rækker
            migrationBuilder.Sql(@"ALTER TABLE ""Feedback""
        ALTER COLUMN ""CreatedAt"" SET DEFAULT (now() at time zone 'utc');");

            // 2) Backfill eksisterende rækker hvor Rating er 0 (i tilfælde af gammel data)
            migrationBuilder.Sql(@"UPDATE ""Feedback"" SET ""Rating"" = 5 WHERE ""Rating"" = 0;");

            // 3) Fjern dubletter på DeviceId før unik indeks oprettes (beholder nyeste række)
            migrationBuilder.Sql(@"
                WITH dups AS (
                  SELECT ""Id""
                  FROM (
                    SELECT ""Id"", ""DeviceId"",
                           ROW_NUMBER() OVER (PARTITION BY ""DeviceId"" ORDER BY ""Id"" DESC) AS rn
                    FROM ""Feedback""
                  ) t
                  WHERE t.rn > 1
                )
                DELETE FROM ""Feedback"" f
                USING dups
                WHERE f.""Id"" = dups.""Id"";
                ");
                migrationBuilder.CreateIndex(
                    name: "IX_Feedback_DeviceId",
                    table: "Feedback",
                    column: "DeviceId",
                    unique: true);
            }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Feedback",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_DeviceId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Feedback");

            migrationBuilder.RenameTable(
                name: "Feedback",
                newName: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Feedbacks",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "FeedbackText",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks",
                column: "Id");
        }
    }
}
