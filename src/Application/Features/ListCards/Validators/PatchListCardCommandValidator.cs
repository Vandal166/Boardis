using Application.Contracts.Keycloak;
using Application.Features.ListCards.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.ListCards.Validators;

public sealed class PatchListCardCommandValidator : AbstractValidator<PatchCardCommand>
{
    public PatchListCardCommandValidator(IKeycloakUserService keycloakUserService, IStringLocalizer<BoardResources> localizer)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardIdNotEmptyGuid"]);

        RuleFor(c => c.BoardListId)
            .NotEmpty().WithMessage(localizer["BoardListIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["BoardListIdNotEmptyGuid"]);

        RuleFor(c => c.CardId)
            .NotEmpty().WithMessage(localizer["CardIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["CardIdNotEmptyGuid"]);

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

        When(c => c.Position.IsSet, () =>
        {
            RuleFor(c => c.Position.Value)
                .GreaterThan(0).WithMessage(localizer["PositionGreaterThanZero"]);
        });

        When(c => c.CompletedAt.IsSet, () =>
        {
            RuleFor(c => c.CompletedAt.Value)
                .Must(date => date is null || date <= DateTime.UtcNow)
                .WithMessage(localizer["CompletedAtCannotBeInFuture"]);
        });
    }
}