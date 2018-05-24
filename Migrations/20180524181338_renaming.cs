using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "ICOOutboundAIRETransactions",
                newName: "OneAirePriceInSatoshies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OneAirePriceInSatoshies",
                table: "ICOOutboundAIRETransactions",
                newName: "Rate");
        }
    }
}
