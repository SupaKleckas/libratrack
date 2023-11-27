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

internal class Program
{
	public async static Task Main(string[] args)
	{
        //Host=localhost;Database=dbLibraTrack;Username=postgres;Password=postgrespw;Port=5432;TrustServerCertificate=true
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //using standart

        var connectionString = new NpgsqlConnectionStringBuilder()
        {
            // The Cloud SQL proxy provides encryption between the proxy and instance.
            SslMode = SslMode.Require,
            Host = Environment.GetEnvironmentVariable("35.234.125.126"), // e.g. '/cloudsql/project:region:instance/.s.PGSQL.5432' /cloudsql/vernal-guide-406312:europe-west3:postgres
            Username = Environment.GetEnvironmentVariable("postgres"), // e.g. 'my-db-user
            Password = Environment.GetEnvironmentVariable("dbpostgres"), // e.g. 'my-db-password'
            Database = Environment.GetEnvironmentVariable("postgres"), // e.g. 'my-database'
            TrustServerCertificate = true
        };
        connectionString.Pooling = true;

        //var config = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: false)
        //        .Build();

        // Read json config into AppSettings.
        //AppSettings = new AppSettings();
        //config.Bind(AppSettings);

        //return WebHost.CreateDefaultBuilder(args)
        //.ConfigureServices(services =>
        //    services.AddGoogleDiagnosticsForAspNetCore(
        //        AppSettings.GoogleCloudSettings.ProjectId,
        //        AppSettings.GoogleCloudSettings.ServiceName,
        //        AppSettings.GoogleCloudSettings.Version))
        //.UseStartup<Startup>()
        //.UsePortEnvironmentVariable();


        DbConnection connection = new NpgsqlConnection(connectionString.ConnectionString);
		connection.Open();

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


