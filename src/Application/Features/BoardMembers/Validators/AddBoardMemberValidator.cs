using Application.Contracts.Keycloak;
using Application.Features.BoardMembers.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.BoardMembers.Validators;

public sealed class AddBoardMemberValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberValidator(IKeycloakUserService keycloakUserService, IStringLocalizer<BoardResources> localizer)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);
        
        RuleFor(x => x.UserIdToAdd)
            .NotEmpty().WithMessage(localizer["UserIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["UserIdNotEmptyGuid"])
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(localizer["UserIdToAddUserNotExist"]);
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["RequestingUserIdNotEmptyGuid"])
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(localizer["RequestingUserIdUserNotExist"]);
    }
}