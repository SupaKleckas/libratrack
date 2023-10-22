using FluentValidation;

namespace LibraTrack.Data.Entities
{
	public class Section
	{
		public int Id { get; set; }
		public required string Title { get; set; }
		public int BookCount { get; set; }

		public required Library Library { get; set; }

	}

	public record SectionDto(int Id, string Title, int BookCount);
	public record CreateSectionDto(string Title);
	public record UpdateSectionDto(string Title);

	public class UpdateSectionDtoValidator : AbstractValidator<UpdateSectionDto>
	{
		public UpdateSectionDtoValidator()
		{
			RuleFor(dto => dto.Title).NotEmpty().NotNull().Length(min: 3, max: 100);
		}
	}
}
