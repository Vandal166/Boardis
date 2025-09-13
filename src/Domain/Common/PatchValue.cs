namespace Domain.Common;

/// <summary>
/// immutable struct PatchValue designed to represent a value that may or may not be set.
/// Useful for implementing partial updates (e.g. PATCH) where distinguishing between a property that is explicitly set to its default value (like null or 0)
/// and a property that is not being updated at all.
/// </summary>
public readonly struct PatchValue<T>
{
    public bool IsSet { get; } // indicates whether the value has been set
    public T Value { get; } // the value, if IsSet is true

    private PatchValue(T value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    public static PatchValue<T> Set(T value) => new(value, true);
    public static PatchValue<T> Unset() => new(default!, false);
}