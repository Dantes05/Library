using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateBookRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRentals_AspNetUsers_UserId1",
                table: "BookRentals");

            migrationBuilder.DropIndex(
                name: "IX_BookRentals_UserId1",
                table: "BookRentals");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BookRentals");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "BookRentals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_BookRentals_UserId",
                table: "BookRentals",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRentals_AspNetUsers_UserId",
                table: "BookRentals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRentals_AspNetUsers_UserId",
                table: "BookRentals");

            migrationBuilder.DropIndex(
                name: "IX_BookRentals_UserId",
                table: "BookRentals");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BookRentals",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "BookRentals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookRentals_UserId1",
                table: "BookRentals",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRentals_AspNetUsers_UserId1",
                table: "BookRentals",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
