using Application.Contracts.Keycloak;
using Application.Features.BoardLists.Commands;
using FluentValidation;

namespace Application.Features.BoardLists.Validators;

public sealed class CreateBoardListCommandValidator : AbstractValidator<CreateBoardListCommand>
{
    public CreateBoardListCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        
        RuleFor(c => c.Position)
            .GreaterThan(0).WithMessage("Position must be a positive integer.")
            .LessThanOrEqualTo(1000).WithMessage("Position must be less than or equal to 1000.");
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user is required.")
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(x => $"Requesting user with ID '{x.RequestingUserId}' does not exist.");
    }
}