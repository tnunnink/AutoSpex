using System;
using System.Diagnostics;
using System.Reflection;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Velopack;

namespace AutoSpex.Client.Pages;

public partial class SettingsAboutPageModel() : PageViewModel("About")
{
    private const string Website = "https://github.com/tnunnink/AutoSpex/";
    private const string Issues = "https://github.com/tnunnink/AutoSpex/issues";
    private const string Releases = "https://github.com/tnunnink/AutoSpex/releases";
    private const string LatestRelease = "https://github.com/tnunnink/AutoSpex/releases/latest";

    public override string Route => "Settings/About";

    public string? CurrentVersion => Assembly.GetEntryAssembly()
        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UpdateAppCommand))]
    private UpdateInfo? _newVersion;

    /// <summary>
    /// Updates the application to the latest version.
    /// </summary>
    /// <remarks>
    /// If an update is available, the method downloads and installs
    /// the update and then restarts the application.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanUpdateApp))]
    private async Task UpdateApp()
    {
        if (NewVersion is null) return;

        try
        {
            var manager = new UpdateManager(LatestRelease);

            await manager.DownloadUpdatesAsync(NewVersion);

            manager.ApplyUpdatesAndRestart(NewVersion);
        }
        catch (Exception e)
        {
            Notifier.ShowError("Application update failed",
                $"Update able to upate application at this time due to error {e.Message}");
        }
    }

    /// <summary>
    /// Determines whether the application can be updated.
    /// </summary>
    /// <returns>True if an update is available, otherwise false.</returns>
    private bool CanUpdateApp() => NewVersion is not null;

    /// <summary>
    /// Checks for updates and notifies the user if an update is available.
    /// </summary>
    [RelayCommand]
    private async Task CheckForUpdtaes()
    {
        try
        {
            var manager = new UpdateManager(LatestRelease);

            var newVersion = await manager.CheckForUpdatesAsync();
            if (newVersion is null)
            {
                Notifier.Notify("No updates available",
                    "You are running the latest version of AutoSpex available. Nice work!");
                return;
            }

            NewVersion = newVersion;
        }
        catch (Exception e)
        {
            Notifier.ShowError("Update check failed", $"Failed to check for updates due to error '{e.Message}'");
        }
    }


    [RelayCommand]
    private void NavigateWebsite()
    {
        try
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Website}") { CreateNoWindow = true });
        }
        catch (Exception e)
        {
            Notifier.ShowError("Request Failed", $"Unable to open site {Website} due to error '{e.Message}'.");
        }
    }

    [RelayCommand]
    private void NavigateReleaseNotes()
    {
        try
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Releases}") { CreateNoWindow = true });
        }
        catch (Exception e)
        {
            Notifier.ShowError("Request Failed", $"Unable to open site {Releases} due to error '{e.Message}'.");
        }
    }

    [RelayCommand]
    private void NavigateIssues()
    {
        try
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Issues}") { CreateNoWindow = true });
        }
        catch (Exception e)
        {
            Notifier.ShowError("Request Failed", $"Unable to open site {Issues} due to error '{e.Message}'.");
        }
    }
}