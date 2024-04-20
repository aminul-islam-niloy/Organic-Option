using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class RiderDelevaryUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_FarmerShop_FarmerShopId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_RiderModel_RiderId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_RiderId",
                table: "Deliveries");

            migrationBuilder.AlterColumn<int>(
                name: "FarmerShopId",
                table: "Deliveries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RiderModelId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_RiderModelId",
                table: "Deliveries",
                column: "RiderModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_FarmerShop_FarmerShopId",
                table: "Deliveries",
                column: "FarmerShopId",
                principalTable: "FarmerShop",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_RiderModel_RiderModelId",
                table: "Deliveries",
                column: "RiderModelId",
                principalTable: "RiderModel",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_FarmerShop_FarmerShopId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_RiderModel_RiderModelId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_RiderModelId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "RiderModelId",
                table: "Deliveries");

            migrationBuilder.AlterColumn<int>(
                name: "FarmerShopId",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_RiderId",
                table: "Deliveries",
                column: "RiderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_FarmerShop_FarmerShopId",
                table: "Deliveries",
                column: "FarmerShopId",
                principalTable: "FarmerShop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_RiderModel_RiderId",
                table: "Deliveries",
                column: "RiderId",
                principalTable: "RiderModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
