using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class AddressOnDelivary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Addresses");

            migrationBuilder.AddColumn<int>(
                name: "CustomerPreAddId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CustomerPreAddId",
                table: "Deliveries",
                column: "CustomerPreAddId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Addresses_CustomerPreAddId",
                table: "Deliveries",
                column: "CustomerPreAddId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Addresses_CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CustomerPreAddId",
                table: "Deliveries");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Addresses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Addresses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
