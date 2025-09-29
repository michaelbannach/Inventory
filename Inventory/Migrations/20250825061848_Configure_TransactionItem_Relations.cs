using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class Configure_TransactionItem_Relations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionItem");

            migrationBuilder.CreateTable(
                name: "TransactionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    StockInId = table.Column<int>(type: "int", nullable: true),
                    StockOutId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionItems", x => x.Id);
                    table.CheckConstraint("CK_TransactionItem_ExactlyOneParent", "((CASE WHEN StockInId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN StockOutId IS NOT NULL THEN 1 ELSE 0 END)) = 1");
                    table.ForeignKey(
                        name: "FK_TransactionItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionItems_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TransactionItems_StockIns_StockInId",
                        column: x => x.StockInId,
                        principalTable: "StockIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionItems_StockOuts_StockOutId",
                        column: x => x.StockOutId,
                        principalTable: "StockOuts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_ItemId",
                table: "TransactionItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_LocationId",
                table: "TransactionItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_StockInId",
                table: "TransactionItems",
                column: "StockInId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_StockOutId",
                table: "TransactionItems",
                column: "StockOutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionItems");

            migrationBuilder.CreateTable(
                name: "TransactionItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockInId = table.Column<int>(type: "int", nullable: true),
                    StockOutId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionItem_StockIns_StockInId",
                        column: x => x.StockInId,
                        principalTable: "StockIns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionItem_StockOuts_StockOutId",
                        column: x => x.StockOutId,
                        principalTable: "StockOuts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItem_StockInId",
                table: "TransactionItem",
                column: "StockInId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItem_StockOutId",
                table: "TransactionItem",
                column: "StockOutId");
        }
    }
}
