using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AutoSpex.Client.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PathValidCharactersAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string path)
            throw new ArgumentException($"Input value '{value}' must be a string to validate.");
        
        return path.IndexOfAny(Path.GetInvalidPathChars()) >= 0
            ? new ValidationResult($"The path '{value}' has invalid IO path characters.")
            : ValidationResult.Success;
    }
}