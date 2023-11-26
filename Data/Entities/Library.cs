using FluentValidation;
using LibraTrack.Auth.Model;
using System.ComponentModel.DataAnnotations;

namespace LibraTrack.Data.Entities
{
    public class Library
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }

        [Required]
        public required string UserId { get; set; }
        public User User { get; set; }
    }

    public record LibraryDto(int Id, string Name, string Address);
    public record CreateLibraryDto(string Name, string Address);
    public record UpdateLibraryDto(string Name);

    public class CreateLibraryDtoValidator : AbstractValidator<CreateLibraryDto>
    {
        public CreateLibraryDtoValidator()
        {
            RuleFor(dto => dto.Name).NotEmpty().NotNull().Length(min: 3, max: 120);
            RuleFor(dto => dto.Address).NotEmpty().NotNull().Length(min: 10, max: 200);
        }
    }

    public class UpdateLibraryDtoValidator : AbstractValidator<UpdateLibraryDto>
    {
        public UpdateLibraryDtoValidator()
        {
            RuleFor(dto => dto.Name).NotEmpty().NotNull().Length(min: 3, max: 120);
        }
    }
}
