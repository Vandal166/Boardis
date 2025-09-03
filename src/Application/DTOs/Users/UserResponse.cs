namespace Application.DTOs.Users;

public sealed record UserResponse
{
    public Guid ID { get; init; }
    public string Username { get; init; } = null!;
}