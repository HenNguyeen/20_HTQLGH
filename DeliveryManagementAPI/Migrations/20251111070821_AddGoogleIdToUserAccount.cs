using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIdToUserAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "UserAccounts");
        }
    }
}
