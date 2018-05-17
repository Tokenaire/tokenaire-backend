using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class brandnew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ICOOutboundAIRETransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressSource = table.Column<string>(nullable: true),
                    TxIdSource = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICOOutboundAIRETransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_TxIdSource_AddressSource",
                table: "ICOOutboundAIRETransactions",
                columns: new[] { "TxIdSource", "AddressSource" },
                unique: true,
                filter: "[TxIdSource] IS NOT NULL AND [AddressSource] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ICOOutboundAIRETransactions");
        }
    }
}
