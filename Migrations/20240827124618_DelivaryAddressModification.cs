using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class DelivaryAddressModification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShopAddressId",
                table: "Deliveries");

            migrationBuilder.AlterColumn<string>(
                name: "ShopAddressId",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomercPreAddId",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopAddressId1",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ShopAddressId1",
                table: "Deliveries",
                column: "ShopAddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId1",
                table: "Deliveries",
                column: "ShopAddressId1",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_ShopAddressId1",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShopAddressId1",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CustomercPreAddId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopAddressId1",
                table: "Deliveries");

            migrationBuilder.AlterColumn<int>(
                name: "ShopAddressId",
                table: "Deliveries",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
    }
}
