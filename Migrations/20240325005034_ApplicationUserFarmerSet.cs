using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class ApplicationUserFarmerSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FarmerUserId",
                table: "FarmerShop",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerShop_FarmerUserId",
                table: "FarmerShop",
                column: "FarmerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerShop_AspNetUsers_FarmerUserId",
                table: "FarmerShop",
                column: "FarmerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarmerShop_AspNetUsers_FarmerUserId",
                table: "FarmerShop");

            migrationBuilder.DropIndex(
                name: "IX_FarmerShop_FarmerUserId",
                table: "FarmerShop");

            migrationBuilder.DropColumn(
                name: "FarmerUserId",
                table: "FarmerShop");
        }
    }
}
