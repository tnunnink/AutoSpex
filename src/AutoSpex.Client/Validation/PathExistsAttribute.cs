using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AutoSpex.Client.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PathExistsAttribute(PathType pathType = PathType.Either)
    : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string path)
            throw new ArgumentException($"Input value '{value}' must be a string to validate.");

        return pathType switch
        {
            PathType.File => ValidateFile(path),
            PathType.Directory => ValidateDirectory(path),
            PathType.Either => ValidatePath(path),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    private static ValidationResult? ValidateFile(string path)
    {
        return !File.Exists(path)
            ? new ValidationResult($"The file '{path}' does not exist.")
            : ValidationResult.Success;
    }

    private static ValidationResult? ValidateDirectory(string path)
    {
        return !Directory.Exists(path)
            ? new ValidationResult($"The directory '{path}' does not exist.")
            : ValidationResult.Success;
    }

    private static ValidationResult? ValidatePath(string path)
    {
        return !Path.Exists(path)
            ? new ValidationResult($"The path '{path}' does not exist.")
            : ValidationResult.Success;
    }
}

public enum PathType
{
    File,
    Directory,
    Either
}