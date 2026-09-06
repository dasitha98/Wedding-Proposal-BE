using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wedding_Proposal_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectionRequestDeclineCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeclineCount",
                table: "ProfileConnectionRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineCount",
                table: "ProfileConnectionRequests");
        }
    }
}
