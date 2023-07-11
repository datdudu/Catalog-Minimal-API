using CatalogMinimalAPI.ApiEndpoints;
using CatalogMinimalAPI.AppServicesExtensions;
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


builder.AddApiSwagger();
builder.AddPersistence();
builder.Services.AddCors();
builder.AddAuthenticationJwt();

var app = builder.Build();

app.MapGet("/", () => "CatalogMinimalAPI 2022").ExcludeFromDescription();


app.MapAuthenticationEndpoint();
app.MapCategoriesEndpoints();
app.MapProductsEndpoints();

// Configure the HTTP request pipeline.

var environment = app.Environment;

app.UseExceptionHandling(environment)
    .UseSwaggerMiddleware()
    .UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
