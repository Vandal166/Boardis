using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.Boards.Validators;

public sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage(localizer["TitleRequired"])
            .MaximumLength(100).WithMessage(localizer["TitleMaxLength", 100]);

        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage(localizer["DescriptionMaxLength", 500])
            .When(c => c.Description != null);

        RuleFor(c => c.OwnerId)
            .NotEmpty().WithMessage(localizer["OwnerIdRequired"])
            .NotEqual(Guid.Empty).WithMessage(localizer["OwnerIdNotEmptyGuid"])
            .MustAsync(async (ownerId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(ownerId, cancellation);
                return result.IsSuccess;
            }).WithMessage(localizer["OwnerIdUserNotExist"]);
    }
}