using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForHinanden.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserBio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // no-op: Bio findes allerede i databasen
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op (lad være med at droppe Bio, da kolonnen er ægte i DB)
        }

    }
}
