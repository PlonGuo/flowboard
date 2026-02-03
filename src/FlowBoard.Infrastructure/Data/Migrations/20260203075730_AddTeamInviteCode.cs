using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamInviteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column without unique constraint first
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Teams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Generate unique invite codes for existing teams
            migrationBuilder.Sql(@"
                UPDATE ""Teams""
                SET ""InviteCode"" = UPPER(SUBSTRING(MD5(RANDOM()::TEXT || ""Id""::TEXT) FROM 1 FOR 8))
                WHERE ""InviteCode"" = '';
            ");

            // Now add the unique index
            migrationBuilder.CreateIndex(
                name: "IX_Teams_InviteCode",
                table: "Teams",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_InviteCode",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Teams");
        }
    }
}
