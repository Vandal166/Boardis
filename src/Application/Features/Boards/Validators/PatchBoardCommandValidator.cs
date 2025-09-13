using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;

namespace Application.Features.Boards.Validators;

public sealed class PatchBoardCommandValidator : AbstractValidator<PatchBoardCommand>
{
    public PatchBoardCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");
        
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

        When(c => c.WallpaperImageId.IsSet, () =>
        {
            RuleFor(c => c.WallpaperImageId.Value)
                .Must(id => id is null || id != Guid.Empty).WithMessage("WallpaperImageId must be a valid GUID or null.");
        });

        When(c => c.Visibility.IsSet, () =>
        {
            RuleFor(c => c.Visibility.Value)
                .NotNull().WithMessage("Visibility cannot be null.")
                .IsInEnum().WithMessage("Invalid visibility level.");
        });
    }
}