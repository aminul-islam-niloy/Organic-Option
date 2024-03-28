using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class OrderSoldTract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSoldDate",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSoldDate",
                table: "FarmerShop",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "ShopRevenue",
                table: "FarmerShop",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SoldQuantity",
                table: "FarmerShop",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSoldDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastSoldDate",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "ShopRevenue",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "SoldQuantity",
                table: "FarmerShop");
        }
    }
}
