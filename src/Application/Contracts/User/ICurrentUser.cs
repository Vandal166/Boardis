namespace Application.Contracts.User;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid Id { get; }
    string Username { get; }
}