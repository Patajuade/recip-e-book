using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipEBook.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PagingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_UpdatedAt_Id",
                table: "ShoppingLists",
                columns: new[] { "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_UpdatedAt_Id",
                table: "Recipes",
                columns: new[] { "UpdatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShoppingLists_UpdatedAt_Id",
                table: "ShoppingLists");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_UpdatedAt_Id",
                table: "Recipes");
        }
    }
}
