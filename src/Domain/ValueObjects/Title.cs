using Vogen;

namespace Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Title
{
    //Validate method that returns Validation
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Title is required.");

        if (value.Length > 100)
            return Validation.Invalid("Title cannot exceed 100 characters.");

        return Validation.Ok;
    }
}