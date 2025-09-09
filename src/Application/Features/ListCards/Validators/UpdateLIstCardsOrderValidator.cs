using Application.Contracts.Keycloak;
using Application.Features.ListCards.Commands;
using FluentValidation;

namespace Application.Features.ListCards.Validators;

public sealed class UpdateLIstCardsOrderValidator : AbstractValidator<UpdateListCardsOrderCommand>
{
    public UpdateLIstCardsOrderValidator(IKeycloakUserService keycloakUserService)
    {
        RuleFor(c => c.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board ID cannot be an empty GUID.");
        
        RuleFor(c => c.BoardListId)
            .NotEmpty().WithMessage("Board List ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Board List ID cannot be an empty GUID.");
        
        RuleFor(c => c.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Requesting User ID cannot be an empty GUID.")
            .MustAsync(async (reqId, cancellation) =>
            {
                var result = await keycloakUserService.GetUserByIdAsync(reqId, cancellation);
                return result.IsSuccess;
            }).WithMessage("User with the given Requesting User ID does not exist.");
        
        RuleFor(c => c.Cards)
            .NotEmpty().WithMessage("Card orders cannot be empty.")
            .Must(cardOrders => cardOrders.All(co => co.CardId != Guid.Empty))
            .WithMessage("Card ID in card orders cannot be an empty GUID.")
            .Must(cardOrders => cardOrders.Select(co => co.Position).Distinct().Count() == cardOrders.Count)
            .WithMessage("Positions in card orders must be unique.")
            .Must(cardOrders => cardOrders.All(co => co.Position >= 0))
            .WithMessage("Positions in card orders must be zero or positive integers.");
    }
}