using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Ip = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ICOOutboundAIRETransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressSource = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    IsSuccessful = table.Column<bool>(nullable: true),
                    TxIdSource = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICOOutboundAIRETransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRegistrationInfos",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RegisteredFromReferralLinkId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegistrationInfos", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    EncryptedSeed = table.Column<string>(nullable: false),
                    ICOBTCAddress = table.Column<string>(nullable: true),
                    LastLoginDate = table.Column<DateTime>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(nullable: true),
                    NormalizedUserName = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    PublicKey = table.Column<string>(nullable: false),
                    RegisteredDate = table.Column<DateTime>(nullable: false),
                    RegisteredFromIP = table.Column<string>(nullable: false),
                    RegistrationInfoUserId = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    Signature = table.Column<string>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserRegistrationInfos_Id",
                        column: x => x.Id,
                        principalTable: "UserRegistrationInfos",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_UserRegistrationInfos_RegistrationInfoUserId",
                        column: x => x.RegistrationInfoUserId,
                        principalTable: "UserRegistrationInfos",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserReferralLinks",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReferralLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReferralLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emails_Value",
                table: "Emails",
                column: "Value",
                unique: true,
                filter: "[Value] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_AddressSource",
                table: "ICOOutboundAIRETransactions",
                columns: new[] { "TxIdSource", "AddressSource" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [AddressSource] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserReferralLinks_UserId",
                table: "UserReferralLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrationInfos_RegisteredFromReferralLinkId",
                table: "UserRegistrationInfos",
                column: "RegisteredFromReferralLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RegistrationInfoUserId",
                table: "Users",
                column: "RegistrationInfoUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRegistrationInfos_UserReferralLinks_RegisteredFromReferralLinkId",
                table: "UserRegistrationInfos",
                column: "RegisteredFromReferralLinkId",
                principalTable: "UserReferralLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReferralLinks_Users_UserId",
                table: "UserReferralLinks");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "ICOOutboundAIRETransactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserRegistrationInfos");

            migrationBuilder.DropTable(
                name: "UserReferralLinks");
        }
    }
}
