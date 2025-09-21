using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Application.Features.Boards.Validators;

public sealed class UploadBoardMediaCommandValidator : AbstractValidator<UploadBoardMediaCommand>
{
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/jpg"
    };

    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

    public UploadBoardMediaCommandValidator(IStringLocalizer<BoardResources> localizer, IKeycloakUserService keycloakUserService)
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

        RuleFor(c => c.File)
            .NotNull().WithMessage(localizer["FileRequired"])
            .Must(file => file != null && file.Length > 0).WithMessage(localizer["FileNotEmpty"])
            .Must(file => file != null && file.Length <= MaxFileSizeInBytes)
                .WithMessage(localizer["FileMaxSize", MaxFileSizeInBytes / (1024 * 1024)])
            .Must(file => file != null && AllowedContentTypes.Contains(file.ContentType))
                .WithMessage(localizer["FileTypeInvalid"]);
    }
}