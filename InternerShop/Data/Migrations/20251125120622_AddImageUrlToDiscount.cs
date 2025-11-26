using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternerShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Discounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Discounts");
        }
    }
}
