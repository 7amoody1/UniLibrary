using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWishItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnrolledDate",
                table: "WishItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsNotified",
                table: "WishItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WishItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WishItems_ProductId",
                table: "WishItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishItems_Products_ProductId",
                table: "WishItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishItems_Products_ProductId",
                table: "WishItems");

            migrationBuilder.DropIndex(
                name: "IX_WishItems_ProductId",
                table: "WishItems");

            migrationBuilder.DropColumn(
                name: "EnrolledDate",
                table: "WishItems");

            migrationBuilder.DropColumn(
                name: "IsNotified",
                table: "WishItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WishItems");
        }
    }
}
