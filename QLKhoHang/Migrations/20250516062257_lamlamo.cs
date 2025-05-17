using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLKhoHang.Migrations
{
    /// <inheritdoc />
    public partial class lamlamo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Warehouse",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Warehouse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Manager",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Warehouse",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "Manager",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Warehouse");
        }
    }
}
