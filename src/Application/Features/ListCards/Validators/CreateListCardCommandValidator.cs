using Application.Contracts.Keycloak;
using Application.Features.ListCards.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.ListCards.Validators;

public sealed class CreateListCardCommandValidator : AbstractValidator<CreateListCardCommand>
{
    public CreateListCardCommandValidator(IKeycloakUserService keycloakUserService, IStringLocalizer<BoardResources> localizer)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);
        
        RuleFor(c => c.BoardListId)
            .NotEmpty().WithMessage(localizer["BoardListIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardListIdNotEmptyGuid"]);
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage(localizer["TitleRequired"])
            .MaximumLength(100).WithMessage(localizer["TitleMaxLength", 100]);

        RuleFor(c => c.Position)
            .GreaterThan(0).WithMessage(localizer["PositionGreaterThanZero"]);
        
        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage(localizer["DescriptionMaxLength", 500])
            .When(c => !string.IsNullOrEmpty(c.Description));
        
        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["RequestingUserIdNotEmptyGuid"])
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage(localizer["RequestingUserIdUserNotExist"]);
    }
}