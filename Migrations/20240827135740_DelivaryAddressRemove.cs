using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class DelivaryAddressRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId1",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShopAddressId1",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CustomercPreAddId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopAddressId1",
                table: "Deliveries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerPreAddId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomercPreAddId",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopAddressId",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopAddressId1",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CustomerPreAddId",
                table: "Deliveries",
                column: "CustomerPreAddId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ShopAddressId1",
                table: "Deliveries",
                column: "ShopAddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_CustomerPreAddId",
                table: "Deliveries",
                column: "CustomerPreAddId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId1",
                table: "Deliveries",
                column: "ShopAddressId1",
                principalTable: "Addresses",
                principalColumn: "Id");
        }
    }
}
