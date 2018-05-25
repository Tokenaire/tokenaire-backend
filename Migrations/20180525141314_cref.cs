using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class cref : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserBTCAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ICOBTCRefundAddress",
                table: "ICOTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ICOBTCRefundAddress",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ICOBTCRefundAddress",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "UserBTCAddress",
                table: "Users",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ICOBTCRefundAddress",
                table: "ICOTransactions",
                nullable: true);
        }
    }
}
