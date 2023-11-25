using LibraTrack.Data.Entities;
using LibraTrack.Data;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using LibraTrack.Auth.Model;
using Microsoft.AspNetCore.Authorization;

namespace LibraTrack
{
	public class BooksEndpoints
	{
		public static void AddBookApi(RouteGroupBuilder booksGroup)
		{
			booksGroup.MapGet("books", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, LibDbContext dbContext, CancellationToken cancellationToken) =>
			{
				return (await dbContext.Books.Include(i => i.Section).Include(i => i.Section.Library).ToListAsync(cancellationToken))
					.Where(i => i.Section.Library.Id == libraryId && i.Section.Id == sectionId)
					.Select(b => new BookDto(b.Id, b.Title, b.PublishYear, b.Publisher, b.Author, b.Gendre));
			});

			booksGroup.MapGet("books/{bookId}", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, int bookId, LibDbContext dbContext) =>
			{
				var book = await dbContext.Books.FirstOrDefaultAsync<Book>(b => b.Id == bookId && b.Section.Id == sectionId && b.Section.Library.Id == libraryId);
				if (book == null)
					return Results.NotFound();

				return Results.Ok(new BookDto(book.Id, book.Title, book.PublishYear, book.Publisher, book.Author, book.Gendre));
			});

			booksGroup.MapPost("books", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, [Validate] CreateBookDto createBookDto, HttpContext httpContext, LibDbContext dbContext) =>
			{
				var section = await dbContext.Sections.FirstOrDefaultAsync<Section>(sec => sec.Id == sectionId);
				if (section == null)
				{
					return Results.NotFound();
				}

				var book = new Book
				{
					Title = createBookDto.Title,
					PublishYear = createBookDto.PublishYear,
					Publisher = createBookDto.Publisher,
					Author = createBookDto.Author,
					Gendre = createBookDto.Gendre,
					Section = section,
					UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
				};

				dbContext.Books.Add(book);
				book.Section.BookCount++;
				await dbContext.SaveChangesAsync();

				return Results.Created($"/api/libraries/{libraryId}/sections/{sectionId}/books/{book.Id}",
					new BookDto(book.Id, book.Title, book.PublishYear, book.Publisher, book.Author, book.Gendre));
			});

			booksGroup.MapPut("books/{bookId}", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, int bookId, [Validate] UpdateBookDto updateBookDto, LibDbContext dbContext) =>
			{
				var book = await dbContext.Books.FirstOrDefaultAsync<Book>(b => b.Id == bookId && b.Section.Id == sectionId && b.Section.Library.Id == libraryId);
				if (book == null)
				{
					return Results.NotFound();
				}

				book.Title = updateBookDto.Title;
				book.Publisher = updateBookDto.Publisher;
				book.Gendre = updateBookDto.Gendre;
				dbContext.Update(book);
				await dbContext.SaveChangesAsync();

				return Results.Ok(new BookDto(book.Id, book.Title, book.PublishYear, book.Publisher, book.Author, book.Gendre));
			});

			booksGroup.MapDelete("books/{bookId}", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, int bookId, LibDbContext dbContext) =>
			{
				var book = await dbContext.Books.FirstOrDefaultAsync<Book>(b => b.Id == bookId && b.Section.Id == sectionId && b.Section.Library.Id == libraryId);
				if (book == null)
				{
					return Results.NotFound();
				}
				var section = await dbContext.Sections.FirstOrDefaultAsync(s => s.Library.Id == libraryId && s.Id == sectionId);
				if (section == null)
				{
					return Results.NotFound();
				}


				dbContext.Remove(book);
				book.Section.BookCount--;
				await dbContext.SaveChangesAsync();

				return Results.NoContent();
			});
		}

	}
}
