using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class renamiko : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ICOOutboundAIRETransactions");

            migrationBuilder.CreateTable(
                name: "ICOTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    ICOBTCAddress = table.Column<string>(nullable: true),
                    ICOBTCRefundAddress = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: true),
                    OneAirePriceInSatoshies = table.Column<long>(nullable: false),
                    ProcessType = table.Column<string>(nullable: true),
                    RegisteredFromReferralLinkId = table.Column<string>(nullable: true),
                    TxIdSource = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false),
                    ValueReceivedInSatoshies = table.Column<long>(nullable: false),
                    ValueSentInAIRE = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICOTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ICOTransactions_UserReferralLinks_RegisteredFromReferralLinkId",
                        column: x => x.RegisteredFromReferralLinkId,
                        principalTable: "UserReferralLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ICOTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ICOTransactions_RegisteredFromReferralLinkId",
                table: "ICOTransactions",
                column: "RegisteredFromReferralLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ICOTransactions_UserId",
                table: "ICOTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ICOTransactions_TxIdSource_ICOBTCAddress",
                table: "ICOTransactions",
                columns: new[] { "TxIdSource", "ICOBTCAddress" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [ICOBTCAddress] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ICOTransactions");

            migrationBuilder.CreateTable(
                name: "ICOOutboundAIRETransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CanProcess = table.Column<bool>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    ICOBTCAddress = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: true),
                    OneAirePriceInSatoshies = table.Column<long>(nullable: false),
                    RegisteredFromReferralLinkId = table.Column<string>(nullable: true),
                    TxIdSource = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false),
                    ValueReceivedInSatoshies = table.Column<long>(nullable: false),
                    ValueSentInAIRE = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICOOutboundAIRETransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ICOOutboundAIRETransactions_UserReferralLinks_RegisteredFromReferralLinkId",
                        column: x => x.RegisteredFromReferralLinkId,
                        principalTable: "UserReferralLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ICOOutboundAIRETransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions",
                column: "RegisteredFromReferralLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_UserId",
                table: "ICOOutboundAIRETransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_ICOBTCAddress",
                table: "ICOOutboundAIRETransactions",
                columns: new[] { "TxIdSource", "ICOBTCAddress" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [ICOBTCAddress] IS NOT NULL");
        }
    }
}
