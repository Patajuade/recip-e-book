using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipEBook.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSearchTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchTitle",
                table: "Recipes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchTitle",
                table: "Recipes");
        }
    }
}
