using Application.Contracts.Keycloak;
using Application.Features.Ollama.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.Ollama.Validators;

public sealed class GenerateListStructureValidator : AbstractValidator<GenerateListStructureCommand>
{
    public GenerateListStructureValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(localizer["DescriptionRequired"])
            .MaximumLength(2000).WithMessage(localizer["DescriptionMaxLength", 2000])
            .Must(description => !string.IsNullOrWhiteSpace(description)).WithMessage(localizer["DescriptionNotWhitespace"]);
        
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