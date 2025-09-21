using Application.Contracts.Keycloak;
using Application.Features.BoardLists.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.BoardLists.Validators;

public sealed class CreateBoardListCommandValidator : AbstractValidator<CreateBoardListCommand>
{
    public CreateBoardListCommandValidator(IKeycloakUserService keycloakUserService, IStringLocalizer<BoardResources> localizer)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage(localizer["TitleRequired"])
            .MaximumLength(100).WithMessage(localizer["TitleMaxLength", 100]);
        
        RuleFor(c => c.Position)
            .GreaterThan(0).WithMessage(localizer["PositionGreaterThanZero"]);
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(localizer["RequestingUserIdUserNotExist"]);
    }
}