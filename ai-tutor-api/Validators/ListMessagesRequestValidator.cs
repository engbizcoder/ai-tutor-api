namespace Ai.Tutor.Api.Validators;

using Ai.Tutor.Api.DTOs;
using FluentValidation;

public sealed class ListMessagesRequestValidator : AbstractValidator<ListMessagesQueryParams>
{
    public ListMessagesRequestValidator()
    {
        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.")
            .WithErrorCode("MESSAGES_001");

        this.RuleFor(x => x.Cursor)
            .MaximumLength(500)
            .WithMessage("Cursor cannot exceed 500 characters.")
            .WithErrorCode("MESSAGES_002")
            .When(x => !string.IsNullOrEmpty(x.Cursor));
    }
}

