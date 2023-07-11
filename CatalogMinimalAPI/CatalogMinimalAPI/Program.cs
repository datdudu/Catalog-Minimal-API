using CatalogMinimalAPI.Context;
using CatalogMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
                                            options
                                            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));   

var app = builder.Build();

app.MapGet("/", () => "CatalogMinimalAPI 2022").ExcludeFromDescription();

//Categories Endpoints

app.MapPost("/Categories", async (Category category, AppDbContext db) =>
{
    db.Categories.Add(category);
    await db.SaveChangesAsync();

    return Results.Created($"/categories/{category.CategoryId}", category);
});

app.MapGet("/Categories", async(AppDbContext db) => await db.Categories.ToListAsync());

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

    if(category is null)
        return Results.NotFound();

    db.Categories.Remove(category);
    db.SaveChangesAsync();

    return Results.NoContent();
});

//Products Endpoints

app.MapPost("/Products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/Products/{product.ProductId}", product);
});

app.MapGet("/Products", async (AppDbContext db) => await db.Products.ToListAsync());

app.MapGet("/Products/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Products.FindAsync(id)
        is Product product
            ? Results.Ok(product)
            : Results.NotFound();
});


app.MapPut("/Products", async (int id, Product product, AppDbContext db) =>
{
    if (product.ProductId != id)
        return Results.BadRequest();

    var productDB = await db.Products.FindAsync(id);

    if (productDB is null) return Results.NotFound();

    productDB.Name = product.Name;
    productDB.Description = product.Description;
    productDB.Price = product.Price;
    productDB.Image = product.Image;
    productDB.BuyDate = product.BuyDate;
    productDB.Stock = product.Stock;
    productDB.CategoryId = product.CategoryId;

    await db.SaveChangesAsync();

    return Results.Ok(productDB);
});

app.MapDelete("/Products/{id:int}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null)
        return Results.NotFound();

    db.Products.Remove(product);
    db.SaveChangesAsync();

    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
