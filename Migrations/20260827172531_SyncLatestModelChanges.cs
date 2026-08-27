using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class SyncLatestModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanExtensionRequest_Loans_LoanId",
                table: "LoanExtensionRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanExtensionRequest_Members_MemberId",
                table: "LoanExtensionRequest");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanExtensionRequest_Loans_LoanId",
                table: "LoanExtensionRequest",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanExtensionRequest_Members_MemberId",
                table: "LoanExtensionRequest",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanExtensionRequest_Loans_LoanId",
                table: "LoanExtensionRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanExtensionRequest_Members_MemberId",
                table: "LoanExtensionRequest");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanExtensionRequest_Loans_LoanId",
                table: "LoanExtensionRequest",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanExtensionRequest_Members_MemberId",
                table: "LoanExtensionRequest",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
