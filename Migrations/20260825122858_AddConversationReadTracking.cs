using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wedding_Proposal_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationReadTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAtA",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAtB",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadAtA",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastReadAtB",
                table: "Conversations");
        }
    }
}
