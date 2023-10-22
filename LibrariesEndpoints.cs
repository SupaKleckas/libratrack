using LibraTrack.Data.Entities;
using LibraTrack.Data;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;

namespace LibraTrack
{
	public class LibrariesEndpoints
	{
		public static void AddLibraryApi(RouteGroupBuilder librariesGroup)
		{
			librariesGroup.MapGet("libraries", async (LibDbContext dbContext, CancellationToken cancellationToken) =>
			{
				return (await dbContext.Libraries.ToListAsync(cancellationToken)).Select(library => new LibraryDto(library.Id, library.Name, library.Address));

			});

			librariesGroup.MapGet("libraries/{libraryId}", async (int libraryId, LibDbContext dbContext) =>
			{
				var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
				if (library == null)
					return Results.NotFound();

				return Results.Ok(new LibraryDto(library.Id, library.Name, library.Address));
			});

			librariesGroup.MapPost("libraries", async ([Validate] CreateLibraryDto createLibraryDto, LibDbContext dbContext) =>
			{
				var library = new Library { Name = createLibraryDto.Name, Address = createLibraryDto.Address };

				dbContext.Libraries.Add(library);
				await dbContext.SaveChangesAsync();

				return Results.Created($"/api/libraries/{library.Id}", new LibraryDto(library.Id, library.Name, library.Address));
			});

			librariesGroup.MapPut("libraries/{libraryId}", async (int libraryId, [Validate] UpdateLibraryDto updateLibraryDto, LibDbContext dbContext) =>
			{
				var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
				if (library == null)
					return Results.NotFound();

				library.Name = updateLibraryDto.Name;
				dbContext.Update(library);
				await dbContext.SaveChangesAsync();

				return Results.Ok(new LibraryDto(library.Id, library.Name, library.Address));
			});

			librariesGroup.MapDelete("libraries/{libraryId}", async (int libraryId, LibDbContext dbContext) =>
			{
				var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
				if (library == null)
					return Results.NotFound();

				dbContext.Remove(library);
				await dbContext.SaveChangesAsync();

				return Results.NoContent();
			});
		}

	}
}
