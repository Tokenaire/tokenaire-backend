using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class renami : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_AddressSource",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.RenameColumn(
                name: "AddressSource",
                table: "ICOOutboundAIRETransactions",
                newName: "ICOBTCAddress");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_ICOBTCAddress",
                table: "ICOOutboundAIRETransactions",
                columns: new[] { "TxIdSource", "ICOBTCAddress" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [ICOBTCAddress] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_ICOBTCAddress",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.RenameColumn(
                name: "ICOBTCAddress",
                table: "ICOOutboundAIRETransactions",
                newName: "AddressSource");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_AddressSource",
                table: "ICOOutboundAIRETransactions",
                columns: new[] { "TxIdSource", "AddressSource" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [AddressSource] IS NOT NULL");
        }
    }
}
