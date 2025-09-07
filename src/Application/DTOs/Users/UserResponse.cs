namespace Application.DTOs.Users;

public sealed record UserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = null!;
}