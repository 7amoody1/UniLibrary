using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ModifyOrderDetailsAndShoppingCartTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndBorrowDate",
                table: "ShoppingCarts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartBorrowDate",
                table: "ShoppingCarts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndBorrowDate",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartBorrowDate",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndBorrowDate",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "StartBorrowDate",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "EndBorrowDate",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "StartBorrowDate",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "OrderDetails");
        }
    }
}
