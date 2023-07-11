using CatalogMinimalAPI.Models;
using CatalogMinimalAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace CatalogMinimalAPI.ApiEndpoints
{
    public static class AuthenticationEndpoints
    {
        public static void MapAuthenticationEndpoint(this WebApplication app)
        {
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
        }
    }
}
