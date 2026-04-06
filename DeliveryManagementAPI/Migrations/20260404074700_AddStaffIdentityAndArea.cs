using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffIdentityAndArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "DeliveryStaffs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hometown",
                table: "DeliveryStaffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdCardNumber",
                table: "DeliveryStaffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkingArea",
                table: "DeliveryStaffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DeliveryStaffs");

            migrationBuilder.DropColumn(
                name: "Hometown",
                table: "DeliveryStaffs");

            migrationBuilder.DropColumn(
                name: "IdCardNumber",
                table: "DeliveryStaffs");

            migrationBuilder.DropColumn(
                name: "WorkingArea",
                table: "DeliveryStaffs");
        }
    }
}
