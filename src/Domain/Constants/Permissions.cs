namespace Domain.Constants;

[Flags]
public enum Permissions
{
    None   = 0,
    Create = 1 << 0,
    Read   = 1 << 1,
    Update = 1 << 2,
    Delete = 1 << 3,
}