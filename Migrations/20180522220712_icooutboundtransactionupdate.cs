using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class icooutboundtransactionupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Rate",
                table: "ICOOutboundAIRETransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ValueReceivedInSatoshies",
                table: "ICOOutboundAIRETransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ValueSentInAIRE",
                table: "ICOOutboundAIRETransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions",
                column: "RegisteredFromReferralLinkId");

            migrationBuilder.AddForeignKey(
                name: "FK_ICOOutboundAIRETransactions_UserReferralLinks_RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions",
                column: "RegisteredFromReferralLinkId",
                principalTable: "UserReferralLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ICOOutboundAIRETransactions_UserReferralLinks_RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropIndex(
                name: "IX_ICOOutboundAIRETransactions_RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropColumn(
                name: "RegisteredFromReferralLinkId",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropColumn(
                name: "ValueReceivedInSatoshies",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropColumn(
                name: "ValueSentInAIRE",
                table: "ICOOutboundAIRETransactions");
        }
    }
}
