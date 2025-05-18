using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLKhoHang.Migrations
{
    /// <inheritdoc />
    public partial class werehouse1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "productCount",
                table: "Warehouse",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "productCount",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Warehouse");
        }
    }
}
