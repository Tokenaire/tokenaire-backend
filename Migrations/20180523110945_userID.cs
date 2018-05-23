using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class userID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ICOOutboundAIRETransactions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ICOOutboundAIRETransactions_UserId",
                table: "ICOOutboundAIRETransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ICOOutboundAIRETransactions_Users_UserId",
                table: "ICOOutboundAIRETransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ICOOutboundAIRETransactions_Users_UserId",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropIndex(
                name: "IX_ICOOutboundAIRETransactions_UserId",
                table: "ICOOutboundAIRETransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ICOOutboundAIRETransactions");
        }
    }
}
