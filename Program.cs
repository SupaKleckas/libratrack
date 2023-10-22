using FluentValidation;
using LibraTrack;
using LibraTrack.Data;
using LibraTrack.Data.Entities;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using System.Data.Common;
using System.Net;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddValidatorsFromAssemblyContaining<Program>();
		builder.Services.AddDbContext<LibDbContext>();
		var app = builder.Build();

		var librariesGroup = app.MapGroup("/api").WithValidationFilter();
		LibrariesEndpoints.AddLibraryApi(librariesGroup);

		var sectionsGroup = app.MapGroup("/api/libraries/{libraryId}").WithValidationFilter();
		SectionsEndpoints.AddSectionApi(sectionsGroup);

		var booksGroup = app.MapGroup("/api/libraries/{libraryId}/sections/{sectionId}").WithValidationFilter();
		BooksEndpoints.AddBookApi(booksGroup);

		app.Run();

		
	}
}


