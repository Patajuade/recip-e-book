using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecipEBook.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    SearchTitle = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Servings = table.Column<int>(type: "integer", nullable: true),
                    PrepTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    CookTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Ingredients = table.Column<string>(type: "jsonb", nullable: false),
                    Steps = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeTag",
                columns: table => new
                {
                    RecipesId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeTag", x => new { x.RecipesId, x.TagsKey });
                    table.ForeignKey(
                        name: "FK_RecipeTag_Recipes_RecipesId",
                        column: x => x.RecipesId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeTag_Tags_TagsKey",
                        column: x => x.TagsKey,
                        principalTable: "Tags",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    Checked = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    SourceRecipeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_Recipes_SourceRecipeId",
                        column: x => x.SourceRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_OwnerId_UpdatedAt_Id",
                table: "Recipes",
                columns: new[] { "OwnerId", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_TagsKey",
                table: "RecipeTag",
                column: "TagsKey");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_ShoppingListId",
                table: "ShoppingListItems",
                column: "ShoppingListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_SourceRecipeId",
                table: "ShoppingListItems",
                column: "SourceRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_OwnerId_UpdatedAt_Id",
                table: "ShoppingLists",
                columns: new[] { "OwnerId", "UpdatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "RecipeTag");

            migrationBuilder.DropTable(
                name: "ShoppingListItems");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "ShoppingLists");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
