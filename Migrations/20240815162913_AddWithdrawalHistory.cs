using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class AddWithdrawalHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiderModel_AspNetUsers_RiderUserId",
                table: "RiderModel");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "RiderModel");

            migrationBuilder.AlterColumn<string>(
                name: "RiderUserId",
                table: "RiderModel",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RiderModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "withdrawalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FarmerShopId = table.Column<int>(type: "int", nullable: true),
                    RiderModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_withdrawalHistories_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_withdrawalHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_withdrawalHistories_FarmerShop_FarmerShopId",
                        column: x => x.FarmerShopId,
                        principalTable: "FarmerShop",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_withdrawalHistories_RiderModel_RiderModelId",
                        column: x => x.RiderModelId,
                        principalTable: "RiderModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_withdrawalHistories_AdminId",
                table: "withdrawalHistories",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawalHistories_FarmerShopId",
                table: "withdrawalHistories",
                column: "FarmerShopId");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawalHistories_RiderModelId",
                table: "withdrawalHistories",
                column: "RiderModelId");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawalHistories_UserId",
                table: "withdrawalHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiderModel_AspNetUsers_RiderUserId",
                table: "RiderModel",
                column: "RiderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiderModel_AspNetUsers_RiderUserId",
                table: "RiderModel");

            migrationBuilder.DropTable(
                name: "withdrawalHistories");

            migrationBuilder.AlterColumn<string>(
                name: "RiderUserId",
                table: "RiderModel",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RiderModel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "RiderModel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RiderModel_AspNetUsers_RiderUserId",
                table: "RiderModel",
                column: "RiderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
