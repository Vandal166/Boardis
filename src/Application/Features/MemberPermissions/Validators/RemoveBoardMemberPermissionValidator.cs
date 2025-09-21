using Application.Contracts.Keycloak;
using Application.Features.MemberPermissions.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.MemberPermissions.Validators;

public sealed class RemoveBoardMemberPermissionValidator : AbstractValidator<RemoveBoardMemberPermissionCommand>
{
    public RemoveBoardMemberPermissionValidator(IKeycloakUserService userService, IStringLocalizer<BoardResources> localizer)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(localizer["BoardIdRequired"]);
        
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage(localizer["MemberIdRequired"]);

        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage(localizer["PermissionRequired"]);
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .MustAsync(async (userId, ct) => 
            {
                var result = await userService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(localizer["RequestingUserIdUserNotExist"]);
    }
}