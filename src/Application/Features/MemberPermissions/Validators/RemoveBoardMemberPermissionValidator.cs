using Application.Contracts.Keycloak;
using Application.Features.MemberPermissions.Commands;
using FluentValidation;

namespace Application.Features.MemberPermissions.Validators;

public sealed class RemoveBoardMemberPermissionValidator : AbstractValidator<RemoveBoardMemberPermissionCommand>
{
    public RemoveBoardMemberPermissionValidator(IKeycloakUserService userService)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId is required.");
        
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("MemberId is required.");

        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission is required.");
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user is required.")
            .MustAsync(async (userId, ct) => 
            {
                var result = await userService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(x => $"Requesting user with ID '{x.RequestingUserId}' does not exist.");
    }
}