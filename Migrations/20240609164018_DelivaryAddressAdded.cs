using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class DelivaryAddressAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopAddress",
                table: "Deliveries");

            migrationBuilder.AddColumn<int>(
                name: "ShopAddressId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ShopAddressId",
                table: "Deliveries",
                column: "ShopAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId",
                table: "Deliveries",
                column: "ShopAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopAddressId",
                table: "Deliveries");

            migrationBuilder.AddColumn<string>(
                name: "ShopAddress",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
