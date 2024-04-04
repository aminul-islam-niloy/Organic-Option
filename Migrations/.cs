using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicOption.Migrations
{
    public partial class FixForeignKeyIssue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmerShopAddress",
                table: "Deliveries");

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAddressId",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FarmerShopAddressId",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryAddressId",
                table: "Deliveries",
                column: "DeliveryAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_FarmerShopAddressId",
                table: "Deliveries",
                column: "FarmerShopAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_DeliveryAddressId",
                table: "Deliveries",
                column: "DeliveryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_FarmerShopAddressId",
                table: "Deliveries",
                column: "FarmerShopAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_DeliveryAddressId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_FarmerShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_DeliveryAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_FarmerShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "DeliveryAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "FarmerShopAddressId",
                table: "Deliveries");

            migrationBuilder.AddColumn<string>(
                name: "FarmerShopAddress",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
