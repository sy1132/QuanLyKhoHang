using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLKhoHang.Migrations
{
    /// <inheritdoc />
    public partial class ImportDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cotst",
                table: "Imports");

            migrationBuilder.RenameColumn(
                name: "Quanty",
                table: "Imports",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Imports",
                newName: "Status");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Imports",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateInput",
                table: "Imports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ImportDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInport = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    IdSupplier = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportDetails");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Imports");

            migrationBuilder.DropColumn(
                name: "DateInput",
                table: "Imports");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Imports",
                newName: "Quanty");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Imports",
                newName: "Name");

            migrationBuilder.AddColumn<float>(
                name: "Cotst",
                table: "Imports",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
