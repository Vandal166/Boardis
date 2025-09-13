using Application.Contracts.Keycloak;
using Application.Features.ListCards.Commands;
using FluentValidation;

namespace Application.Features.ListCards.Validators;

public sealed class PatchListCardCommandValidator : AbstractValidator<PatchCardCommand>
{
    public PatchListCardCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");

        RuleFor(c => c.BoardListId)
            .NotEmpty().WithMessage("Board List ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board List ID cannot be an empty GUID.");

        RuleFor(c => c.CardId)
            .NotEmpty().WithMessage("Card ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Card ID cannot be an empty GUID.");

        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Requesting User ID cannot be an empty GUID.")
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage("User with the given Requesting User ID does not exist.");

        When(c => c.Title.IsSet, () =>
        {
            RuleFor(c => c.Title.Value)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        });

        When(c => c.Description.IsSet, () =>
        {
            RuleFor(c => c.Description.Value)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        });

        When(c => c.Position.IsSet, () =>
        {
            RuleFor(c => c.Position.Value)
                .GreaterThan(0).WithMessage("Position must be greater than zero.");
        });

        When(c => c.CompletedAt.IsSet, () =>
        {
            RuleFor(c => c.CompletedAt.Value)
                .Must(date => date is null || date <= DateTime.UtcNow)
                .WithMessage("CompletedAt cannot be set to a future date.");
        });
    }
}