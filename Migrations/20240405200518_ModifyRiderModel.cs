using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganicOption.Migrations
{
    public partial class ModifyRiderModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Riders_RiderId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Riders_RiderId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "Riders");

            migrationBuilder.AddColumn<bool>(
                name: "RiderStatus",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RiderModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    RiderStatus = table.Column<bool>(type: "bit", nullable: false),
                    DrivingLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiderAddressId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    BagType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiderModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiderModel_Addresses_RiderAddressId",
                        column: x => x.RiderAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiderModel_AspNetUsers_RiderUserId",
                        column: x => x.RiderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiderModel_RiderAddressId",
                table: "RiderModel",
                column: "RiderAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_RiderModel_RiderUserId",
                table: "RiderModel",
                column: "RiderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_RiderModel_RiderId",
                table: "Deliveries",
                column: "RiderId",
                principalTable: "RiderModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_RiderModel_RiderId",
                table: "Shifts",
                column: "RiderId",
                principalTable: "RiderModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_RiderModel_RiderId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_RiderModel_RiderId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "RiderModel");

            migrationBuilder.DropColumn(
                name: "RiderStatus",
                table: "Shifts");

            migrationBuilder.CreateTable(
                name: "Riders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiderAddressId = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    BagType = table.Column<int>(type: "int", nullable: false),
                    DrivingLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    NID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Riders_Addresses_RiderAddressId",
                        column: x => x.RiderAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Riders_RiderAddressId",
                table: "Riders",
                column: "RiderAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Riders_RiderId",
                table: "Deliveries",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Riders_RiderId",
                table: "Shifts",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
