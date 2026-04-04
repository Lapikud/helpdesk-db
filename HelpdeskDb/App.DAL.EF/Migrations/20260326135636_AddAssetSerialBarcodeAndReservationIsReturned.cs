using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetSerialBarcodeAndReservationIsReturned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "AssetReservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "AssetReservations");
        }
    }
}
