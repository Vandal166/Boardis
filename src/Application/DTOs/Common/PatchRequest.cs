namespace Application.DTOs.Common;

/// <summary>
/// An abstract base class for patch requests that tracks which properties have been set.
/// </summary>
public abstract class PatchRequest
{
    private readonly HashSet<string> _properties = new HashSet<string>();

    public bool HasProperty(string propertyName) => _properties.Contains(propertyName);

    protected void SetHasProperty(string propertyName) => _properties.Add(propertyName);
}