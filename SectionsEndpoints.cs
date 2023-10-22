using LibraTrack.Data.Entities;
using LibraTrack.Data;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.ComponentModel.Design;

namespace LibraTrack
{
	public class SectionsEndpoints
	{
		public static void AddSectionApi(RouteGroupBuilder sectionsGroup)
		{
			sectionsGroup.MapGet("sections", async (int libraryId, LibDbContext dbContext, CancellationToken cancellationToken) =>
			{
				return (await dbContext.Sections.Include(s => s.Library).ToListAsync(cancellationToken))
					.Where(s => s.Library.Id == libraryId)
					.Select(section => new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapGet("sections/{sectionId}", async (int libraryId, int sectionId, LibDbContext dbContext) =>
			{
				var section = await dbContext.Sections.FirstOrDefaultAsync<Section>(s => s.Library.Id == libraryId && s.Id == sectionId);
				if (section == null)
					return Results.NotFound();

				return Results.Ok(new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapPost("sections", async (int libraryId, [Validate] UpdateSectionDto createSectionDto, LibDbContext dbContext) =>
			{
				var library = await dbContext.Libraries.FirstOrDefaultAsync<Library>(lib => lib.Id == libraryId);
				if (library == null)
				{
					return Results.NotFound();
				}

				var section = new Section
				{
					Title = createSectionDto.Title,
					BookCount = 0,
					Library = library
				};

				dbContext.Sections.Add(section);
				await dbContext.SaveChangesAsync();

				return Results.Created($"/api/libraries/{library.Id}/sections/{section.Id}", new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapPut("sections/{sectionId}", async (int libraryId, int sectionId, [Validate] UpdateSectionDto updateSectionDto, LibDbContext dbContext) =>
			{
				var section = await dbContext.Sections.FirstOrDefaultAsync(s => s.Library.Id == libraryId && s.Id == sectionId);
				if (section == null)
				{
					return Results.NotFound();
				}

				section.Title = updateSectionDto.Title;
				dbContext.Update(section);
				await dbContext.SaveChangesAsync();

				return Results.Ok(new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapDelete("sections/{sectionId}", async (int libraryId, int sectionId, LibDbContext dbContext) =>
			{
				var section = await dbContext.Sections.FirstOrDefaultAsync(s => s.Library.Id == libraryId && s.Id == sectionId);
				if (section == null)
				{
					return Results.NotFound();
				}

				dbContext.Remove(section);
				await dbContext.SaveChangesAsync();

				return Results.NoContent();
			});
		}
	}
}
