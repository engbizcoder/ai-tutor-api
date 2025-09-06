namespace Ai.Tutor.Api.Validators;

using Contracts.DTOs;
using FluentValidation;

public sealed class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    public CreateMessageRequestValidator()
    {
        this.RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required.")
            .WithErrorCode("MESSAGES_003")
            .MaximumLength(10000)
            .WithMessage("Message content cannot exceed 10,000 characters.")
            .WithErrorCode("MESSAGES_004");

        this.RuleFor(x => x.SenderType)
            .IsInEnum()
            .WithMessage("Invalid sender type.")
            .WithErrorCode("MESSAGES_005");

        this.RuleFor(x => x.SenderId)
            .NotEmpty()
            .WithMessage("Sender ID is required.")
            .WithErrorCode("MESSAGES_006")
            .When(x => x.SenderType == Contracts.Enums.SenderType.User);

        this.RuleFor(x => x.MetadataJson)
            .MaximumLength(5000)
            .WithMessage("Metadata JSON cannot exceed 5,000 characters.")
            .WithErrorCode("MESSAGES_007")
            .When(x => !string.IsNullOrEmpty(x.MetadataJson));

        this.RuleFor(x => x.IdempotencyKey)
            .MaximumLength(255)
            .WithMessage("Idempotency key cannot exceed 255 characters.")
            .WithErrorCode("MESSAGES_008")
            .When(x => !string.IsNullOrEmpty(x.IdempotencyKey));
    }
}
