using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class RenamePropertyIdToPropertyDefintionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValues_Items_ItemId",
                table: "PropertyValues");

            migrationBuilder.DropIndex(
                name: "IX_PropertyValues_ItemId",
                table: "PropertyValues");

            migrationBuilder.DropIndex(
                name: "IX_ItemTypeProperty_ItemTypeId",
                table: "ItemTypeProperty");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "PropertyValues",
                newName: "PropertyDefinitionId");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "PropertyValues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_ItemId_LocationId",
                table: "TransactionItems",
                columns: new[] { "ItemId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_ItemId_StockInId",
                table: "TransactionItems",
                columns: new[] { "ItemId", "StockInId" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValues_ItemId_PropertyDefinitionId",
                table: "PropertyValues",
                columns: new[] { "ItemId", "PropertyDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValues_PropertyDefinitionId",
                table: "PropertyValues",
                column: "PropertyDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTypeProperty_ItemTypeId_PropertyDefinitionId",
                table: "ItemTypeProperty",
                columns: new[] { "ItemTypeId", "PropertyDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValues_Items_ItemId",
                table: "PropertyValues",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValues_PropertyDefinition_PropertyDefinitionId",
                table: "PropertyValues",
                column: "PropertyDefinitionId",
                principalTable: "PropertyDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValues_Items_ItemId",
                table: "PropertyValues");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValues_PropertyDefinition_PropertyDefinitionId",
                table: "PropertyValues");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_ItemId_LocationId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_ItemId_StockInId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_PropertyValues_ItemId_PropertyDefinitionId",
                table: "PropertyValues");

            migrationBuilder.DropIndex(
                name: "IX_PropertyValues_PropertyDefinitionId",
                table: "PropertyValues");

            migrationBuilder.DropIndex(
                name: "IX_ItemTypeProperty_ItemTypeId_PropertyDefinitionId",
                table: "ItemTypeProperty");

            migrationBuilder.RenameColumn(
                name: "PropertyDefinitionId",
                table: "PropertyValues",
                newName: "PropertyId");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "PropertyValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValues_ItemId",
                table: "PropertyValues",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTypeProperty_ItemTypeId",
                table: "ItemTypeProperty",
                column: "ItemTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValues_Items_ItemId",
                table: "PropertyValues",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
