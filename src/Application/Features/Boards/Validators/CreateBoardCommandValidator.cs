using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;

namespace Application.Features.Boards.Validators;

public sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(c => c.Description != null);

        RuleFor(c => c.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Owner ID cannot be an empty GUID.")
            .MustAsync(async (ownerId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(ownerId, cancellation);
                return result.IsSuccess;
            }).WithMessage("User with the specified Owner ID does not exist.");
    }
}