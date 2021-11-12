using Microsoft.EntityFrameworkCore.Migrations;

namespace RedDog.Bootstrapper.Migrations
{
    public partial class ImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "OrderItem",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StoreLocation",
                columns: table => new
                {
                    StoreId = table.Column<string>(type: "nvarchar(54)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    StateProvince = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(54)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(12,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(12,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreLocation", x => x.StoreId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreLocation");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "OrderItem");
        }
    }
}
