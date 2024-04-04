using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class AddressAddOnModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerAddressId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NID",
                table: "FarmerShop",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopAddressId",
                table: "FarmerShop",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopCatagoryId",
                table: "FarmerShop",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerAddressId",
                table: "Orders",
                column: "CustomerAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerShop_ShopAddressId",
                table: "FarmerShop",
                column: "ShopAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerShop_ShopCatagoryId",
                table: "FarmerShop",
                column: "ShopCatagoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerShop_Addresses_ShopAddressId",
                table: "FarmerShop",
                column: "ShopAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerShop_ProductTypes_ShopCatagoryId",
                table: "FarmerShop",
                column: "ShopCatagoryId",
                principalTable: "ProductTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_CustomerAddressId",
                table: "Orders",
                column: "CustomerAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarmerShop_Addresses_ShopAddressId",
                table: "FarmerShop");

            migrationBuilder.DropForeignKey(
                name: "FK_FarmerShop_ProductTypes_ShopCatagoryId",
                table: "FarmerShop");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_CustomerAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_FarmerShop_ShopAddressId",
                table: "FarmerShop");

            migrationBuilder.DropIndex(
                name: "IX_FarmerShop_ShopCatagoryId",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "CustomerAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NID",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "ShopAddressId",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "ShopCatagoryId",
                table: "FarmerShop");
        }
    }
}
