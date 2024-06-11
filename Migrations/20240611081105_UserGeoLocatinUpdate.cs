using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class UserGeoLocatinUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarmerShop_ProductTypes_ShopCatagoryId",
                table: "FarmerShop");

            migrationBuilder.DropIndex(
                name: "IX_FarmerShop_ShopCatagoryId",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "ShopCatagoryId",
                table: "FarmerShop");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderAcceptTime",
                table: "Deliveries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AspNetUsers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AspNetUsers",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderAcceptTime",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ShopCatagoryId",
                table: "FarmerShop",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerShop_ShopCatagoryId",
                table: "FarmerShop",
                column: "ShopCatagoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerShop_ProductTypes_ShopCatagoryId",
                table: "FarmerShop",
                column: "ShopCatagoryId",
                principalTable: "ProductTypes",
                principalColumn: "Id");
        }
    }
}
