using Microsoft.EntityFrameworkCore;
using O9d.AspNet.FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LibraTrack.Auth.Model;
using Microsoft.AspNetCore.Authorization;
using LibraTrack.Data.Entities;
using LibraTrack.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace LibraTrack
{
    public class LibrariesEndpoints
    {
        public static void AddLibraryApi(RouteGroupBuilder librariesGroup)
        {
            librariesGroup.MapGet("libraries", [Authorize(Roles = Roles.User)] async (LibDbContext dbContext, CancellationToken cancellationToken) =>
            {
                return (await dbContext.Libraries.ToListAsync(cancellationToken)).Select(library => new LibraryDto(library.Id, library.Name, library.Address));

            });

            librariesGroup.MapGet("libraries/{libraryId}", [Authorize(Roles = Roles.User)] async (int libraryId, LibDbContext dbContext) =>
            {
                var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
                if (library == null)
                    return Results.NotFound();

                return Results.Ok(new LibraryDto(library.Id, library.Name, library.Address));
            });

            librariesGroup.MapPost("libraries", [Authorize(Roles = Roles.Admin)] async ([Validate] CreateLibraryDto createLibraryDto, HttpContext httpContext, LibDbContext dbContext) =>
            {
                var library = new Library
                {
                    Name = createLibraryDto.Name,
                    Address = createLibraryDto.Address,
                    UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                };

                dbContext.Libraries.Add(library);
                await dbContext.SaveChangesAsync();

                return Results.Created($"/api/libraries/{library.Id}", new LibraryDto(library.Id, library.Name, library.Address));
            });

            librariesGroup.MapPut("libraries/{libraryId}", [Authorize(Roles = Roles.Admin)] async (int libraryId, [Validate] UpdateLibraryDto updateLibraryDto, LibDbContext dbContext) =>
            {
                var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
                if (library == null)
                    return Results.NotFound("Library not found");

                library.Name = updateLibraryDto.Name;
                dbContext.Update(library);
                await dbContext.SaveChangesAsync();

                return Results.Ok(new LibraryDto(library.Id, library.Name, library.Address));
            });

            librariesGroup.MapDelete("libraries/{libraryId}", [Authorize(Roles = Roles.Admin)] async (int libraryId, LibDbContext dbContext) =>
            {
                var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
                if (library == null)
                    return Results.NotFound("Library not found");

                dbContext.Remove(library);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });

			//librariesGroup.MapPut("libraries/{libraryId}/addWorker", [Authorize(Roles = Roles.Admin)] async (int libraryId, HttpContext httpContext, UserManager<User> userManager, [Validate] SetUserDto setDto, LibDbContext dbContext) =>
			//{

			//	var user = await userManager.FindByNameAsync(setDto.UserName);
			//	if (user == null)
			//	{
			//		return Results.NotFound("User not registered");
			//	}

			//	var library = await dbContext.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);

			//	if (library == null)
			//	{
			//		return Results.NotFound("Library not found");
			//	}

			//	if (httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != library.UserId)
			//	{
			//		return Results.Forbid();
			//	}

			//	user.AssignedLibrary = libraryId;

			//	await dbContext.SaveChangesAsync();
			//	return Results.Ok(new SetUserDto(UserName: setDto.UserName));


			//});
		}

		//public record SetUserDto(string UserName);
		//public class SetLibraryValidator : AbstractValidator<SetUserDto>
		//{
		//	public SetLibraryValidator()
		//	{
		//		RuleFor(dto => dto.UserName).NotEmpty().NotNull();
		//	}
		//}

	}
}
