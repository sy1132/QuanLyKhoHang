using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLKhoHang.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAndModifiedDateToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timeupdate",
                table: "Products",
                newName: "modifiedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "createdDate",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdDate",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "modifiedDate",
                table: "Products",
                newName: "Timeupdate");
        }
    }
}
