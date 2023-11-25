using LibraTrack.Data.Entities;
using LibraTrack.Data;
using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LibraTrack.Auth.Model;
using Microsoft.AspNetCore.Authorization;

namespace LibraTrack
{
	public class SectionsEndpoints
	{
		public static void AddSectionApi(RouteGroupBuilder sectionsGroup)
		{
			sectionsGroup.MapGet("sections", [Authorize(Roles = Roles.User)] async (int libraryId, LibDbContext dbContext, CancellationToken cancellationToken) =>
			{
				return (await dbContext.Sections.Include(s => s.Library).ToListAsync(cancellationToken))
					.Where(s => s.Library.Id == libraryId)
					.Select(section => new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapGet("sections/{sectionId}", [Authorize(Roles = Roles.User)] async (int libraryId, int sectionId, LibDbContext dbContext) =>
			{
				var section = await dbContext.Sections.FirstOrDefaultAsync<Section>(s => s.Library.Id == libraryId && s.Id == sectionId);
				if (section == null)
					return Results.NotFound();

				return Results.Ok(new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapPost("sections", [Authorize(Roles = Roles.Admin)] async (int libraryId, [Validate] UpdateSectionDto createSectionDto, HttpContext httpContext, LibDbContext dbContext) =>
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
					Library = library,
                    UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                };

				dbContext.Sections.Add(section);
				await dbContext.SaveChangesAsync();

				return Results.Created($"/api/libraries/{library.Id}/sections/{section.Id}", new SectionDto(section.Id, section.Title, section.BookCount));
			});

			sectionsGroup.MapPut("sections/{sectionId}", [Authorize(Roles = Roles.Admin)] async (int libraryId, int sectionId, [Validate] UpdateSectionDto updateSectionDto, LibDbContext dbContext) =>
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

			sectionsGroup.MapDelete("sections/{sectionId}", [Authorize(Roles = Roles.Admin)] async (int libraryId, int sectionId, LibDbContext dbContext) =>
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
