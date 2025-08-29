using Application.Features.Boards.Commands;
using FluentValidation;

namespace Application.Features.Boards.Validators;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(c => c.Description != null);
        
        RuleFor(c => c.WallpaperImageId)
            .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("WallpaperImageId must be a valid GUID.")
            .When(c => c.WallpaperImageId != null);

        RuleFor(c => c.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Owner ID cannot be an empty GUID.");
    }
}