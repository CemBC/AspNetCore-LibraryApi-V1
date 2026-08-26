using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanExtensionRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanExtensionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanExtensionRequest_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanExtensionRequest_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtensionRequest_LoanId",
                table: "LoanExtensionRequest",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtensionRequest_MemberId",
                table: "LoanExtensionRequest",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanExtensionRequest");
        }
    }
}
