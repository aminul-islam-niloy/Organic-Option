using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class RiderOrderOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnDeliaryByOffer",
                table: "RiderModel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOfferedToRider",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderCondition",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentCondition",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PayableMoney",
                table: "Deliveries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentCondition",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnDeliaryByOffer",
                table: "RiderModel");

            migrationBuilder.DropColumn(
                name: "IsOfferedToRider",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderCondition",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentCondition",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PayableMoney",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "PaymentCondition",
                table: "Deliveries");
        }
    }
}
