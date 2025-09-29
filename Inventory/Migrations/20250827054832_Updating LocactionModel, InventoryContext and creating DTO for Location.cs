using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingLocactionModelInventoryContextandcreatingDTOforLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bin",
                table: "Locations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RackNo",
                table: "Locations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Room",
                table: "Locations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Room_RackNo_Bin",
                table: "Locations",
                columns: new[] { "Room", "RackNo", "Bin" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_Room_RackNo_Bin",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Bin",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "RackNo",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Room",
                table: "Locations");
        }
    }
}
