using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLKhoHang.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "modifiedDate",
                table: "Products",
                newName: "finaldDate");

            migrationBuilder.RenameColumn(
                name: "Num",
                table: "Products",
                newName: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "finaldDate",
                table: "Products",
                newName: "modifiedDate");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "Num");
        }
    }
}
