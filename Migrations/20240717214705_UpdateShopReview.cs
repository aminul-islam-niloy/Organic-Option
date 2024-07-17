using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class UpdateShopReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopReview_FarmerShop_FarmerShopId1",
                table: "ShopReview");

            migrationBuilder.DropIndex(
                name: "IX_ShopReview_FarmerShopId1",
                table: "ShopReview");

            migrationBuilder.DropColumn(
                name: "FarmerShopId1",
                table: "ShopReview");

            migrationBuilder.AlterColumn<int>(
                name: "FarmerShopId",
                table: "ShopReview",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ShopReview",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "OverallRating",
                table: "FarmerShop",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "FarmerShop",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShopReview_FarmerShopId",
                table: "ShopReview",
                column: "FarmerShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopReview_FarmerShop_FarmerShopId",
                table: "ShopReview",
                column: "FarmerShopId",
                principalTable: "FarmerShop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopReview_FarmerShop_FarmerShopId",
                table: "ShopReview");

            migrationBuilder.DropIndex(
                name: "IX_ShopReview_FarmerShopId",
                table: "ShopReview");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ShopReview");

            migrationBuilder.DropColumn(
                name: "OverallRating",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "FarmerShop");

            migrationBuilder.AlterColumn<string>(
                name: "FarmerShopId",
                table: "ShopReview",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FarmerShopId1",
                table: "ShopReview",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopReview_FarmerShopId1",
                table: "ShopReview",
                column: "FarmerShopId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopReview_FarmerShop_FarmerShopId1",
                table: "ShopReview",
                column: "FarmerShopId1",
                principalTable: "FarmerShop",
                principalColumn: "Id");
        }
    }
}
