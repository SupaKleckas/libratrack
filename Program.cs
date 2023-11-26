using FluentValidation;
using System.Text;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using System.Data.Common;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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

internal class Program
{
	public async static Task Main(string[] args)
	{
		JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //using standart

        var connectionString = new NpgsqlConnectionStringBuilder()
        {
            // The Cloud SQL proxy provides encryption between the proxy and instance.
            SslMode = SslMode.Require,

            // Note: Saving credentials in environment variables is convenient, but not
            // secure - consider a more secure solution such as
            // Cloud Secret Manager (https://cloud.google.com/secret-manager) to help
            // keep secrets safe.
            Host = Environment.GetEnvironmentVariable("/cloudsql/vernal-guide-406312:europe-west3:postgres"), // e.g. '/cloudsql/project:region:instance'
            Username = Environment.GetEnvironmentVariable("postgres"), // e.g. 'my-db-user
            Password = Environment.GetEnvironmentVariable("dbpostgres"), // e.g. 'my-db-password'
            Database = Environment.GetEnvironmentVariable("postgres"), // e.g. 'my-database'
			TrustServerCertificate = true
        };
        connectionString.Pooling = true;


        var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddValidatorsFromAssemblyContaining<Program>();
		builder.Services.AddDbContext<LibDbContext>();
		builder.Services.AddTransient<JwtTokenService>();
        builder.Services.AddScoped<AuthDbSeeder>();

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        var url = $"http://0.0.0.0:{port}";
        var target = Environment.GetEnvironmentVariable("TARGET") ?? "World";

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
			options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])); //If somethings wrong, look at appsettings SECRET
        });

		//builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

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
		var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();

		await dbSeeder.SeedAsync();

		app.Run(url);
	}
}


