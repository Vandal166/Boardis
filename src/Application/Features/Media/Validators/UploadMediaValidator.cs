using Application.Contracts.Keycloak;
using Application.Features.Media.Commands;
using FluentValidation;

namespace Application.Features.Media.Validators;

public sealed class UploadMediaValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.EntityId)
            .NotEmpty()
                .WithMessage("EntityId is required.");
        
        RuleFor(x => x.File)
            .NotNull()
                .WithMessage("File is required.")
            .Must(file => file.Length > 0)
                .WithMessage("File must not be empty.")
            .Must(file => file.Length <= 5 * 1024 * 1024)
                .WithMessage("File size must not exceed 5 MB.")
            .Must(file => file.ContentType is not null)
                .WithMessage("File must have a valid content type.");
            
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("RequestingUserId is required")
            .NotEqual(Guid.Empty).WithMessage("RequestingUserId cannot be empty GUID")
            .MustAsync(async (id, ct) => 
            {
                var userResult = await keycloakUserService.GetUserByIdAsync(id, ct);
                return userResult.IsSuccess;
            })
            .WithMessage("Requesting user does not exist");
    }
}