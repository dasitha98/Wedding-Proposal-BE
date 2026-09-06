using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wedding_Proposal_BE.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Profiles",
                newName: "Residency_District");

            migrationBuilder.RenameColumn(
                name: "Bio",
                table: "Profiles",
                newName: "AboutMe");

            migrationBuilder.AddColumn<int>(
                name: "Assets_Status",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BasicInfo_Caste",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BasicInfo_Gender",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BasicInfo_HeightLabel",
                table: "Profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BasicInfo_MaritalStatus",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BasicInfo_Race",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BasicInfo_Religion",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Education_Qualification",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Education_QualificationStatus",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Family_FatherOccupation",
                table: "Profiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Family_MotherOccupation",
                table: "Profiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Family_SiblingCount",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "Hobbies",
                table: "Profiles",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Interests",
                table: "Profiles",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGold",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Lifestyle_Alcohol",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lifestyle_Smoking",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManagedBy",
                table: "Profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profession_IncomeRange",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Profession_JobStatus",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Profession_Occupation",
                table: "Profiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Residency_City",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Residency_Country",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Verification_EducationVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verification_EmailVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verification_IdentityVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verification_PhoneVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verification_PhotoVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verification_ProfessionVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserBId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResendAvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttemptsRemaining = table.Column<int>(type: "integer", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileConnectionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConnectionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileFavourites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileFavourites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfilePhoto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Uri = table.Column<string>(type: "text", nullable: false),
                    IsBlurred = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfilePhoto_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sibling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: false),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: false),
                    Occupation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sibling", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sibling_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserAId_UserBId",
                table: "Conversations",
                columns: new[] { "UserAId", "UserBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SentAt",
                table: "Messages",
                columns: new[] { "ConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetOtps_Email",
                table: "PasswordResetOtps",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileConnectionRequests_RequesterUserId_TargetUserId",
                table: "ProfileConnectionRequests",
                columns: new[] { "RequesterUserId", "TargetUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileFavourites_UserId_TargetProfileId",
                table: "ProfileFavourites",
                columns: new[] { "UserId", "TargetProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfilePhoto_ProfileId",
                table: "ProfilePhoto",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Sibling_ProfileId",
                table: "Sibling",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PasswordResetOtps");

            migrationBuilder.DropTable(
                name: "ProfileConnectionRequests");

            migrationBuilder.DropTable(
                name: "ProfileFavourites");

            migrationBuilder.DropTable(
                name: "ProfilePhoto");

            migrationBuilder.DropTable(
                name: "Sibling");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropColumn(
                name: "Assets_Status",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_Caste",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_Gender",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_HeightLabel",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_MaritalStatus",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_Race",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "BasicInfo_Religion",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Education_Qualification",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Education_QualificationStatus",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Family_FatherOccupation",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Family_MotherOccupation",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Family_SiblingCount",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Hobbies",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "IsGold",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Lifestyle_Alcohol",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Lifestyle_Smoking",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ManagedBy",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Profession_IncomeRange",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Profession_JobStatus",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Profession_Occupation",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Residency_City",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Residency_Country",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_EducationVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_EmailVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_IdentityVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_PhoneVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_PhotoVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Verification_ProfessionVerified",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Residency_District",
                table: "Profiles",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "AboutMe",
                table: "Profiles",
                newName: "Bio");
        }
    }
}
