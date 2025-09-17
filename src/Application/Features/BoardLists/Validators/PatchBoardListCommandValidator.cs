using System.Text.RegularExpressions;
using Application.Contracts.Keycloak;
using Application.Features.BoardLists.Commands;
using FluentValidation;

namespace Application.Features.BoardLists.Validators;

public sealed class PatchBoardListCommandValidator : AbstractValidator<PatchBoardListCommand>
{
    public PatchBoardListCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage("BoardId is required")
            .NotEqual(Guid.Empty).WithMessage("BoardId cannot be empty GUID");
        
        RuleFor(x => x.BoardListId)
            .NotEmpty().WithMessage("BoardListId is required")
            .NotEqual(Guid.Empty).WithMessage("BoardListId cannot be empty GUID");
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("RequestingUserId is required")
            .NotEqual(Guid.Empty).WithMessage("RequestingUserId cannot be empty GUID")
            .MustAsync(async (id, ct) => 
                {
                    var userResult = await keycloakUserService.GetUserByIdAsync(id, ct);
                    return userResult.IsSuccess;
                })
            .WithMessage("Requesting user does not exist");
        
        When(c => c.Title.IsSet, () =>
        {
            RuleFor(c => c.Title.Value)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        });

        When(c => c.Position.IsSet, () =>
        {
            RuleFor(c => c.Position.Value)
                .GreaterThan(0).WithMessage("Position must be greater than zero.");
        });
    }
}