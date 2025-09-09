using Application.Contracts.Keycloak;
using Application.Features.ListCards.Commands;
using FluentValidation;

namespace Application.Features.ListCards.Validators;

public sealed class UpdateListCardCommandValidator : AbstractValidator<UpdateListCardCommand>
{
    public UpdateListCardCommandValidator(IKeycloakUserService keycloakUserService)
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
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        
        RuleFor(c => c.Position)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be zero or a positive integer.");
        
        RuleFor(c => c.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 500 characters.")
            .When(c => !string.IsNullOrEmpty(c.Description));
        
        
        RuleFor(c => c.CompletedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CompletedAt cannot be in the future.")
            .When(c => c.CompletedAt is not null);
        
        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Requesting User ID cannot be an empty GUID.")
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage("User with the given Requesting User ID does not exist.");
    }
}