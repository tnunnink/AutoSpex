using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoSpex.Persistence.Settings;
using Avalonia.Styling;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public sealed class Settings(IMediator mediator)
{
    /// <summary>
    /// Retrieves a setting value for the specified key, deserialized to the specified type.
    /// </summary>
    /// <param name="key">The key identifying the setting to retrieve.</param>
    /// <typeparam name="TValue">The type of the setting value to be deserialized and returned.</typeparam>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized value of the setting.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the setting is not found or cannot be deserialized to the specified type.
    /// </exception>
    public async Task<TValue> GetValue<TValue>(SettingKey key)
    {
        var json = await mediator.Send(new ReadSetting(key.ToString()));

        if (json is null)
            throw new InvalidOperationException($"Setting {key} is required but was not found.");

        var value = JsonSerializer.Deserialize<TValue>(json);

        if (value is null)
            throw new InvalidOperationException($"Setting {key} could not be deserialized to {typeof(TValue).Name}.");

        return value;
    }

    /// <summary>
    /// Retrieves the value of a specified setting key, deserialized to the specified type, or returns the
    /// default value if the setting does not exist.
    /// </summary>
    /// <param name="key">The setting key identifying the value to be retrieved.</param>
    /// <typeparam name="TValue">The type to which the setting value should be deserialized and returned.</typeparam>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized setting value
    /// or the default value if the setting is not found.</returns>
    public async Task<TValue?> GetValueOrDefault<TValue>(SettingKey key)
    {
        var json = await mediator.Send(new ReadSetting(key.ToString()));
        return json is null ? default : JsonSerializer.Deserialize<TValue>(json);
    }

    /// <summary>
    /// Saves a setting value for the specified key.
    /// </summary>
    /// <param name="key">The key representing the setting to be saved.</param>
    /// <param name="value">The value to be saved associated with the key.</param>
    /// <typeparam name="TValue">The type of the value to be saved.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SaveValue<TValue>(SettingKey key, TValue value)
    {
        var json = JsonSerializer.Serialize(value);
        return mediator.Send(new SaveSetting(key.ToString(), json));
    }
}

/// <summary>
/// Represents a key for identifying specific application settings.
/// </summary>
public enum SettingKey
{
    Theme,
    ShellHeight,
    ShellWidth,
    ShellX,
    ShellY,
    ShellState,
    AlwaysDiscardChanges
}

public static class SettingsExtensions
{
    /// <summary>
    /// Reads the theme setting value and maps it to a corresponding ThemeVariant.
    /// </summary>
    /// <param name="settings">The instance of the Settings service to read the theme from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ThemeVariant corresponding to the theme setting.</returns>
    public static async Task<ThemeVariant> GetTheme(this Settings settings)
    {
        var value = await settings.GetValue<string>(SettingKey.Theme);

        return value switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}