using CatalogMinimalAPI.Context;
using CatalogMinimalAPI.Models;
using CatalogMinimalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalogMinimalApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                       Enter 'Bearer'[space] and then your token in the text input below.
                       Example: \'Bearer 12345abcdef\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
                                            options
                                            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication
                (JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", () => "CatalogMinimalAPI 2022").ExcludeFromDescription();
//Endpoint to Login
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel == null)
        return Results.BadRequest("Invalid Login");

    if (userModel.UserName == "duduzin" && userModel.Password == "dudu@1")
    {
        var tokenString = tokenService.GetToken(app.Configuration["Jwt:key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            userModel);

        return Results.Ok(new { token = tokenString });
    }
    else
    {
        return Results.BadRequest("Invalid Login");
    }

}).Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status200OK)
                .WithName("Login")
                .WithTags("Authentication");

//Categories Endpoints

app.MapPost("/Categories", async (Category category, AppDbContext db) =>
{
    db.Categories.Add(category);
    await db.SaveChangesAsync();

    return Results.Created($"/categories/{category.CategoryId}", category);
});

app.MapGet("/Categories", async(AppDbContext db) => await db.Categories.ToListAsync()).WithTags("Categories").RequireAuthorization();

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

app.MapGet("/Products", async (AppDbContext db) => await db.Products.ToListAsync()).WithTags("Products").RequireAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.Run();
