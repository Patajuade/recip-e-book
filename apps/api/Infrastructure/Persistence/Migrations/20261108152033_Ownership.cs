using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipEBook.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Ownership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShoppingLists_UpdatedAt_Id",
                table: "ShoppingLists");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_UpdatedAt_Id",
                table: "Recipes");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ShoppingLists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_OwnerId_UpdatedAt_Id",
                table: "ShoppingLists",
                columns: new[] { "OwnerId", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_OwnerId_UpdatedAt_Id",
                table: "Recipes",
                columns: new[] { "OwnerId", "UpdatedAt", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_AspNetUsers_OwnerId",
                table: "Recipes",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingLists_AspNetUsers_OwnerId",
                table: "ShoppingLists",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_AspNetUsers_OwnerId",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingLists_AspNetUsers_OwnerId",
                table: "ShoppingLists");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingLists_OwnerId_UpdatedAt_Id",
                table: "ShoppingLists");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_OwnerId_UpdatedAt_Id",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_UpdatedAt_Id",
                table: "ShoppingLists",
                columns: new[] { "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_UpdatedAt_Id",
                table: "Recipes",
                columns: new[] { "UpdatedAt", "Id" });
        }
    }
}
