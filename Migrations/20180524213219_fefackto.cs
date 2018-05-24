using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class fefackto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "ICOOutboundAIRETransactions",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "ICOOutboundAIRETransactions");
        }
    }
}
