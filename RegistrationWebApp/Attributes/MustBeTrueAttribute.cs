using System.ComponentModel.DataAnnotations;

namespace RegistrationWebApp.Attributes;

/// <summary>
/// Attribute to ensure boolean values must be true when validated.
/// </summary>
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is bool b && b;
}