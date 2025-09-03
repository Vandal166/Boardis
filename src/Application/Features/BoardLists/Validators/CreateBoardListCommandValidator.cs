using Application.Features.BoardLists.Commands;
using FluentValidation;

namespace Application.Features.BoardLists.Validators;

public sealed class CreateBoardListCommandValidator : AbstractValidator<CreateBoardListCommand>
{
    public CreateBoardListCommandValidator()
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
    }
}