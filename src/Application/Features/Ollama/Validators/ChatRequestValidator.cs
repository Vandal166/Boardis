using Application.Contracts.Keycloak;
using Application.Features.Ollama.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.Ollama.Validators;

public sealed class ChatRequestValidator : AbstractValidator<ChatRequestCommand>
{
    public ChatRequestValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage(localizer["BoardIdRequired"]);
        
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(localizer["MessageRequired"])
            .MaximumLength(1000).WithMessage(localizer["MessageMaxLength", 1000])
            .Must(message => !string.IsNullOrWhiteSpace(message)).WithMessage(localizer["MessageNotWhitespace"]);
        
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