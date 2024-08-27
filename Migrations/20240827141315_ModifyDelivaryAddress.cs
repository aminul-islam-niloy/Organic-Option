using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class ModifyDelivaryAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerCurrentAddressId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopCurrentAddressId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CustomerCurrentAddressId",
                table: "Deliveries",
                column: "CustomerCurrentAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ShopCurrentAddressId",
                table: "Deliveries",
                column: "ShopCurrentAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_CustomerCurrentAddressId",
                table: "Deliveries",
                column: "CustomerCurrentAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_ShopCurrentAddressId",
                table: "Deliveries",
                column: "ShopCurrentAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_CustomerCurrentAddressId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_ShopCurrentAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_CustomerCurrentAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShopCurrentAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CustomerCurrentAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopCurrentAddressId",
                table: "Deliveries");
        }
    }
}
