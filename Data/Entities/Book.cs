using FluentValidation;
using Npgsql.PostgresTypes;
using System.Globalization;

namespace LibraTrack.Data.Entities
{
	public class Book
	{
		public int Id { get; set; }
		public required string Title { get; set; }
		public required int PublishYear { get; set; }
		public required string Publisher { get; set; }
		public required string Author { get; set; }
		public required string Gendre { get; set; }
		public required Section Section { get; set; }

	}

	public record BookDto(int Id, string Title, int PublishYear, string Publisher, string Author, string Gendre);
	public record CreateBookDto(string Title, int PublishYear, string Publisher, string Author, string Gendre);
	public record UpdateBookDto(string Title, string Publisher, string Gendre);

	public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
	{
		public CreateBookDtoValidator()
		{
			RuleFor(dto => dto.Title).NotEmpty().NotNull().Length(min: 3, max: 120);
			RuleFor(dto => dto.PublishYear).NotEmpty().NotNull();
			RuleFor(dto => dto.Publisher).NotEmpty().NotNull().Length(min: 3, max: 200);
			RuleFor(dto => dto.Author).NotEmpty().NotNull().Length(min: 1, max: 100);
			RuleFor(dto => dto.Gendre).NotEmpty().NotNull().Length(min: 3, max: 50);
		}
	}

	public class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
	{
		public UpdateBookDtoValidator()
		{
			RuleFor(dto => dto.Title).NotEmpty().NotNull().Length(min: 3, max: 120);
			RuleFor(dto => dto.Publisher).NotEmpty().NotNull().Length(min: 3, max: 200);
			RuleFor(dto => dto.Gendre).NotEmpty().NotNull().Length(min: 3, max: 50);
		}
	}
}
