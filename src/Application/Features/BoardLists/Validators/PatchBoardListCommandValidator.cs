using System.Text.RegularExpressions;
using Application.Contracts.Keycloak;
using Application.Features.BoardLists.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.BoardLists.Validators;

public sealed class PatchBoardListCommandValidator : AbstractValidator<PatchBoardListCommand>
{
    public PatchBoardListCommandValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);
        
        RuleFor(x => x.BoardListId)
            .NotEmpty().WithMessage(localizer["BoardListIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardListIdNotEmptyGuid"]);
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["RequestingUserIdNotEmptyGuid"])
            .MustAsync(async (id, ct) => 
                {
                    var userResult = await keycloakUserService.GetUserByIdAsync(id, ct);
                    return userResult.IsSuccess;
                })
            .WithMessage(localizer["RequestingUserIdUserNotExist"]);
        
        When(c => c.Title.IsSet, () =>
        {
            RuleFor(c => c.Title.Value)
                .NotEmpty().WithMessage(localizer["TitleRequired"])
                .MaximumLength(100).WithMessage(localizer["TitleMaxLength", 100]);
        });

        When(c => c.Position.IsSet, () =>
        {
            RuleFor(c => c.Position.Value)
                .GreaterThan(0).WithMessage(localizer["PositionGreaterThanZero"]);
        });
    }
}