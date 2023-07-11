using CatalogMinimalAPI.Context;
using CatalogMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogMinimalAPI.ApiEndpoints
{
    public static class CategoriesEndpoints
    {
        public static void MapCategoriesEndpoints(this WebApplication app)
        {
            app.MapPost("/Categories", async (Category category, AppDbContext db) =>
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();

                return Results.Created($"/categories/{category.CategoryId}", category);
            });

            app.MapGet("/Categories", async (AppDbContext db) => await db.Categories.ToListAsync()).WithTags("Categories").RequireAuthorization();

            app.MapGet("/Categories/{id:int}", async (int id, AppDbContext db) =>
            {
                return await db.Categories.FindAsync(id)
                    is Category category
                        ? Results.Ok(category)
                        : Results.NotFound();
            });


            app.MapPut("/Categories", async (int id, Category category, AppDbContext db) =>
            {
                if (category.CategoryId != id)
                    return Results.BadRequest();

                var categoryDB = await db.Categories.FindAsync(id);

                if (categoryDB is null) return Results.NotFound();

                categoryDB.Name = category.Name;
                categoryDB.Description = category.Description;

                await db.SaveChangesAsync();

                return Results.Ok(categoryDB);
            });

            app.MapDelete("/Categories/{id:int}", async (int id, AppDbContext db) =>
            {
                var category = await db.Categories.FindAsync(id);

                if (category is null)
                    return Results.NotFound();

                db.Categories.Remove(category);
                db.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
