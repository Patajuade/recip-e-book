using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecipEBook.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TagCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Free-text tags from before the catalog can't be mapped to its keys: remove them
            // first, before the tables are rebuilt.
            migrationBuilder.Sql("""DELETE FROM "RecipeTag"; DELETE FROM "Tags";""");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Tags_TagsId",
                table: "RecipeTag");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Name",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tags",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tags",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "RecipeTag",
                newName: "TagsKey");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeTag_TagsId",
                table: "RecipeTag",
                newName: "IX_RecipeTag_TagsKey");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Key", "Category" },
                values: new object[,]
                {
                    { "african", "Cuisine" },
                    { "air-fryer", "CookingMethod" },
                    { "american", "Cuisine" },
                    { "appetizer", "Course" },
                    { "autumn", "Season" },
                    { "barbecue", "CookingMethod" },
                    { "batch-cooking", "Occasion" },
                    { "beef", "MainIngredient" },
                    { "birthday", "Occasion" },
                    { "bread", "Course" },
                    { "breakfast", "Course" },
                    { "brunch", "Course" },
                    { "brunch-party", "Occasion" },
                    { "budget", "Practical" },
                    { "buffet", "Occasion" },
                    { "caribbean", "Cuisine" },
                    { "cheese", "MainIngredient" },
                    { "chinese", "Cuisine" },
                    { "chocolate", "MainIngredient" },
                    { "christmas", "Occasion" },
                    { "comfort-food", "Occasion" },
                    { "dairy-free", "Diet" },
                    { "date-night", "Occasion" },
                    { "dessert", "Course" },
                    { "drink", "Course" },
                    { "easter", "Occasion" },
                    { "easy", "Practical" },
                    { "egg-free", "Diet" },
                    { "eggs", "MainIngredient" },
                    { "everyday", "Occasion" },
                    { "fish", "MainIngredient" },
                    { "freezer-friendly", "Practical" },
                    { "french", "Cuisine" },
                    { "fried", "CookingMethod" },
                    { "fruit", "MainIngredient" },
                    { "gluten-free", "Diet" },
                    { "grains", "MainIngredient" },
                    { "greek", "Cuisine" },
                    { "grilled", "CookingMethod" },
                    { "halal", "Diet" },
                    { "high-protein", "Diet" },
                    { "indian", "Cuisine" },
                    { "italian", "Cuisine" },
                    { "japanese", "Cuisine" },
                    { "kids", "Occasion" },
                    { "korean", "Cuisine" },
                    { "kosher", "Diet" },
                    { "lactose-free", "Diet" },
                    { "lamb", "MainIngredient" },
                    { "legumes", "MainIngredient" },
                    { "low-calorie", "Diet" },
                    { "low-carb", "Diet" },
                    { "low-fat", "Diet" },
                    { "low-salt", "Diet" },
                    { "lunchbox", "Occasion" },
                    { "main-course", "Course" },
                    { "make-ahead", "Practical" },
                    { "mediterranean", "Cuisine" },
                    { "mexican", "Cuisine" },
                    { "microwave", "CookingMethod" },
                    { "middle-eastern", "Cuisine" },
                    { "mushrooms", "MainIngredient" },
                    { "new-year", "Occasion" },
                    { "no-cook", "CookingMethod" },
                    { "north-african", "Cuisine" },
                    { "nut-free", "Diet" },
                    { "one-pot", "CookingMethod" },
                    { "oven", "CookingMethod" },
                    { "party", "Occasion" },
                    { "pasta", "MainIngredient" },
                    { "pescatarian", "Diet" },
                    { "picnic", "Occasion" },
                    { "pork", "MainIngredient" },
                    { "portuguese", "Cuisine" },
                    { "potatoes", "MainIngredient" },
                    { "poultry", "MainIngredient" },
                    { "pressure-cooker", "CookingMethod" },
                    { "quick", "Practical" },
                    { "raw", "CookingMethod" },
                    { "rice", "MainIngredient" },
                    { "salad", "Course" },
                    { "sauce", "Course" },
                    { "seafood", "MainIngredient" },
                    { "side-dish", "Course" },
                    { "slow-cooked", "CookingMethod" },
                    { "snack", "Course" },
                    { "soup", "Course" },
                    { "spanish", "Cuisine" },
                    { "spring", "Season" },
                    { "starter", "Course" },
                    { "steamed", "CookingMethod" },
                    { "stovetop", "CookingMethod" },
                    { "sugar-free", "Diet" },
                    { "summer", "Season" },
                    { "thai", "Cuisine" },
                    { "veal", "MainIngredient" },
                    { "vegan", "Diet" },
                    { "vegetables", "MainIngredient" },
                    { "vegetarian", "Diet" },
                    { "vietnamese", "Cuisine" },
                    { "winter", "Season" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Tags_TagsKey",
                table: "RecipeTag",
                column: "TagsKey",
                principalTable: "Tags",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Tags_TagsKey",
                table: "RecipeTag");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "african");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "air-fryer");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "american");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "appetizer");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "autumn");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "barbecue");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "batch-cooking");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "beef");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "birthday");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "bread");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "breakfast");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "brunch");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "brunch-party");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "budget");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "buffet");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "caribbean");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "cheese");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "chinese");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "chocolate");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "christmas");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "comfort-food");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "dairy-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "date-night");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "dessert");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "drink");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "easter");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "easy");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "egg-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "eggs");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "everyday");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "fish");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "freezer-friendly");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "french");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "fried");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "fruit");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "gluten-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "grains");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "greek");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "grilled");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "halal");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "high-protein");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "indian");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "italian");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "japanese");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "kids");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "korean");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "kosher");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "lactose-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "lamb");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "legumes");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "low-calorie");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "low-carb");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "low-fat");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "low-salt");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "lunchbox");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "main-course");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "make-ahead");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "mediterranean");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "mexican");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "microwave");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "middle-eastern");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "mushrooms");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "new-year");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "no-cook");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "north-african");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "nut-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "one-pot");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "oven");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "party");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "pasta");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "pescatarian");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "picnic");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "pork");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "portuguese");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "potatoes");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "poultry");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "pressure-cooker");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "quick");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "raw");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "rice");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "salad");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "sauce");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "seafood");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "side-dish");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "slow-cooked");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "snack");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "soup");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "spanish");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "spring");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "starter");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "steamed");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "stovetop");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "sugar-free");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "summer");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "thai");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "veal");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "vegan");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "vegetables");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "vegetarian");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "vietnamese");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Key",
                keyValue: "winter");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Tags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Tags",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TagsKey",
                table: "RecipeTag",
                newName: "TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeTag_TagsKey",
                table: "RecipeTag",
                newName: "IX_RecipeTag_TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Tags_TagsId",
                table: "RecipeTag",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
