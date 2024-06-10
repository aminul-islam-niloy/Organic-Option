using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class GoogleMapLatLonOnRider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryLat",
                table: "Deliveries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLon",
                table: "Deliveries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ShopLat",
                table: "Deliveries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ShopLon",
                table: "Deliveries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryLat",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "DeliveryLon",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopLat",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShopLon",
                table: "Deliveries");
        }
    }
}
