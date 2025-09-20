using Application.Contracts.Keycloak;
using Application.Features.Boards.Commands;
using FluentValidation;

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

    public UploadBoardMediaCommandValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");

        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Requesting User ID cannot be an empty GUID.")
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage("User with the given Requesting User ID does not exist.");

        RuleFor(c => c.File)
            .NotNull().WithMessage("File is required.")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty.")
            .Must(file => file.Length <= MaxFileSizeInBytes).WithMessage($"File size cannot exceed {MaxFileSizeInBytes / (1024 * 1024)} MB.")
            .Must(file => AllowedContentTypes.Contains(file.ContentType)).WithMessage("Unsupported file type. Allowed types are: JPEG, PNG, GIF.");
    }
}