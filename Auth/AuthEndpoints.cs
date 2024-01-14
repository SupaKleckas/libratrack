using Microsoft.AspNetCore.Identity;
using LibraTrack.Auth.Model;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using Microsoft.AspNetCore.Http;

namespace LibraTrack.Auth
{
    public static class AuthEndpoints
    {
        public static void AddAuthApi(this WebApplication app)
        {
            //register
            app.MapPost("api/register", async (UserManager<User> userManager, RegisterUserDto registerUserDto) =>
            {
                //check user exists
                var user = await userManager.FindByNameAsync(registerUserDto.Username);
                if (user != null)
                    return Results.UnprocessableEntity("Username already taken.");

                var newUser = new User
                {
                    Email = registerUserDto.Email,
                    UserName = registerUserDto.Username
                };

                var createUserResult = await userManager.CreateAsync(newUser, registerUserDto.Password);
                if (!createUserResult.Succeeded)
                {
                    //var message = string.Join(", ", createUserResult.Errors.Select(x => "Code " + x.Code + " Description" + x.Description));
                    //Console.WriteLine(message);
                    return Results.UnprocessableEntity();
                }

                await userManager.AddToRoleAsync(newUser, Roles.User);

                return Results.Created("api/login", new UserDto(newUser.Id, newUser.Email, newUser.UserName));
            });


            //login
            app.MapPost("api/login", async (UserManager<User> userManager, JwtTokenService jwtTokenService, LoginDto loginDto) =>
            {
                //user validation
                var user = await userManager.FindByNameAsync(loginDto.Username);
                if (user == null)
                    return Results.UnprocessableEntity("Username or password was incorrect.");

                var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
                if(!isPasswordValid)
                    return Results.UnprocessableEntity("Username or password was incorrect.");

                user.forceRelogin = false;
                await userManager.UpdateAsync(user);

                //generate tokens
                var roles = await userManager.GetRolesAsync(user);

                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);

                //Console.WriteLine(user.UserName + " assigned to " + user.AssignedLibrary + " id");
                return Results.Ok(new SuccessfulLoginDto(accessToken, refreshToken, user.AssignedLibrary));
            });

            //accesstoken
            app.MapPost("api/accessToken", async (UserManager<User> userManager, JwtTokenService jwtTokenService, RefreshAccessTokenDto refreshAccessTokenDto) =>
            {
                if(!jwtTokenService.TryParseRefreshToken(refreshAccessTokenDto.RefreshToken, out var claims))
                    return Results.UnprocessableEntity();               

                var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return Results.UnprocessableEntity("Invalid token");

                if (user.forceRelogin)
                    return Results.UnprocessableEntity();

                var roles = await userManager.GetRolesAsync(user);

                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);

                return Results.Ok(new SuccessfulLoginDto(accessToken, refreshToken, user.AssignedLibrary));
            });

            //logout
            app.MapPost("api/logout", async (UserManager<User> userManager, SignInManager<User> signInManager, JwtTokenService jwtTokenService, HttpContext httpContext) =>
            {
                httpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
                await signInManager.SignOutAsync();
                return Results.Ok();
            });
        }

        public record UserDto(string UserId, string Email, string Username);
        public record LoginDto(string Username, string Password);
        public record SuccessfulLoginDto(string AccessToken, string RefreshToken, int? AssignedLibrary);
        public record SuccessfulLogoutDto(string Username);
        public record RefreshAccessTokenDto(string RefreshToken);
        public record RegisterUserDto(string Username, string Email, string Password);
    }
}
