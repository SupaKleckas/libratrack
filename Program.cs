using FluentValidation;
using System.Text;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using System.Data.Common;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using LibraTrack;
using LibraTrack.Auth;
using LibraTrack.Data;
using LibraTrack.Auth.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore.Metadata;

internal class Program
{
    public async static Task Main(string[] args)
	{
        //Host=localhost;Database=dbLibraTrack;Username=postgres;Password=postgrespw;Port=5432;TrustServerCertificate=true
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CORSPolicy", builder =>
            {
                builder
				.AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins("http://localhost:3000", "https://coral-app-4hvmu.ondigitalocean.app/");
            });
        });

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
		builder.Services.AddDbContext<LibDbContext>();
		builder.Services.AddTransient<JwtTokenService>();
        builder.Services.AddScoped<AuthDbSeeder>();

        builder.Services.AddIdentity<User, IdentityRole>()
			.AddEntityFrameworkStores<LibDbContext>()
			.AddDefaultTokenProviders();

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
            options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
			options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]));
        });

        builder.Services.AddAuthorization();

		var app = builder.Build();

        app.UseCors("CORSPolicy");

        var librariesGroup = app.MapGroup("/api").WithValidationFilter();
		LibrariesEndpoints.AddLibraryApi(librariesGroup);

		var sectionsGroup = app.MapGroup("/api/libraries/{libraryId}").WithValidationFilter();
		SectionsEndpoints.AddSectionApi(sectionsGroup);

		var booksGroup = app.MapGroup("/api/libraries/{libraryId}/sections/{sectionId}").WithValidationFilter();
		BooksEndpoints.AddBookApi(booksGroup);

		app.AddAuthApi();

		app.UseAuthentication();
		app.UseAuthorization();

		using var scope = app.Services.CreateScope();

		//var dbContext = scope.ServiceProvider.GetRequiredService<LibDbContext>();
		//dbContext.Database.Migrate();

		var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();

		await dbSeeder.SeedAsync();

		app.Run();
	}
}


