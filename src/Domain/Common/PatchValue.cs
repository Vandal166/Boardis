namespace Domain.Common;

public readonly struct PatchValue<T>
{
    public bool IsSet { get; }
    public T Value { get; }

    private PatchValue(T value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    public static PatchValue<T> Set(T value) => new(value, true);
    public static PatchValue<T> Unset() => new(default!, false);
}