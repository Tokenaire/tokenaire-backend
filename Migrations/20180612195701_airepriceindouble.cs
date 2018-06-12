using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class airepriceindouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "OneAirePriceInSatoshies",
                table: "ICOTransactions",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "OneAirePriceInSatoshies",
                table: "ICOTransactions",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
