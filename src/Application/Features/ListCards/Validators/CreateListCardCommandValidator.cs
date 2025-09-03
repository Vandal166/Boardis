using Application.Features.ListCards.Commands;
using FluentValidation;

namespace Application.Features.ListCards.Validators;

public sealed class CreateListCardCommandValidator : AbstractValidator<CreateListCardCommand>
{
    public CreateListCardCommandValidator()
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");
        
        RuleFor(c => c.BoardListId)
            .NotEmpty().WithMessage("Board List ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board List ID cannot be an empty GUID.");
        
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
    }
}