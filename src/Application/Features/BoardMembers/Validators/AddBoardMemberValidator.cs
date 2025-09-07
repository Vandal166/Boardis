using Application.Contracts.Keycloak;
using Application.Features.BoardMembers.Commands;
using FluentValidation;

namespace Application.Features.BoardMembers.Validators;

public sealed class AddBoardMemberValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberValidator(IKeycloakRoleService keycloakRoleService, IKeycloakUserService keycloakUserService)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage("Board is required.")
            .NotEqual(Guid.Empty).WithMessage("ID cannot be an empty GUID.");
        
        RuleFor(x => x.UserIdToAdd)
            .NotEmpty().WithMessage("User to add is required.")
            .NotEqual(Guid.Empty).WithMessage("ID cannot be an empty GUID.")
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(x => $"User with ID '{x.UserIdToAdd}' does not exist.");
        
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user is required.")
            .NotEqual(Guid.Empty).WithMessage("ID cannot be an empty GUID.")
            .MustAsync(async (userId, ct) => 
            {
                var result = await keycloakUserService.GetUserByIdAsync(userId, ct);
                return result.IsSuccess;
            })
            .WithMessage(x => $"Requesting user with ID '{x.RequestingUserId}' does not exist.");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .MustAsync(async (role, ct) => 
            {
                var result = await keycloakRoleService.RoleExistsAsync(role, ct);
                return result.IsSuccess;
            })
            .WithMessage(x => $"Role '{x.Role}' does not exist.");
    }
}