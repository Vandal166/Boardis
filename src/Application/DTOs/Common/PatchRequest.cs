namespace Application.DTOs.Common;

public abstract class PatchRequest
{
    private readonly HashSet<string> _properties = new HashSet<string>();

    public bool HasProperty(string propertyName) => _properties.Contains(propertyName);

    protected void SetHasProperty(string propertyName) => _properties.Add(propertyName);
}