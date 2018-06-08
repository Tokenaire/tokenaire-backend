using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace tokenairebackend.Migrations
{
    public partial class kyc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ICOKyc",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicantId = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICOKyc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ICOKyc_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ICOKyc_UserId_IsSuccessful",
                table: "ICOKyc",
                columns: new[] { "UserId", "IsSuccessful" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ICOKyc");
        }
    }
}
