using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class UpdateInventoryItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "InventoryItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductsId",
                table: "InventoryItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_ProductsId",
                table: "InventoryItem",
                column: "ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Products_ProductsId",
                table: "InventoryItem",
                column: "ProductsId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Products_ProductsId",
                table: "InventoryItem");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_ProductsId",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "ProductsId",
                table: "InventoryItem");
        }
    }
}
