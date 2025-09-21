using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.Boards.Validators;

public sealed class PatchBoardCommandValidator : AbstractValidator<PatchBoardCommand>
{
    public PatchBoardCommandValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);
        
        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage(localizer["RequestingUserIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["RequestingUserIdNotEmptyGuid"])
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage(localizer["RequestingUserIdUserNotExist"]);

        When(c => c.Title.IsSet, () =>
        {
            RuleFor(c => c.Title.Value)
                .NotEmpty().WithMessage(localizer["TitleRequired"])
                .MaximumLength(100).WithMessage(localizer["TitleMaxLength", 100]);
        });

        When(c => c.Description.IsSet, () =>
        {
            RuleFor(c => c.Description.Value)
                .MaximumLength(500).WithMessage(localizer["DescriptionMaxLength", 500]);
        });
        
        When(c => c.Visibility.IsSet, () =>
        {
            RuleFor(c => c.Visibility.Value)
                .NotNull().WithMessage(localizer["VisibilityRequired"])
                .IsInEnum().WithMessage(localizer["VisibilityInvalid"]);
        });
    }
}