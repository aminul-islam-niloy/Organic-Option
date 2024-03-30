using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class InventoryUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Products_ProductId",
                table: "InventoryItem");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_ProductId",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "InventoryItem");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "InventoryItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "InventoryItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_OrderId",
                table: "InventoryItem",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Orders_OrderId",
                table: "InventoryItem",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Orders_OrderId",
                table: "InventoryItem");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_OrderId",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "InventoryItem");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "InventoryItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_ProductId",
                table: "InventoryItem",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Products_ProductId",
                table: "InventoryItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
