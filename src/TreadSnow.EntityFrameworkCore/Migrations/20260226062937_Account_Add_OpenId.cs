using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreadSnow.Migrations
{
    /// <inheritdoc />
    public partial class Account_Add_OpenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenId",
                table: "AppAccounts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenId",
                table: "AppAccounts");
        }
    }
}
