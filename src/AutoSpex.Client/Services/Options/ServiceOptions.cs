// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace AutoSpex.Client.Services.Options;

/// <summary>
/// Options for telling the Bootstrapper how to register services - basically which services to register the mock
/// implementation as opposed to the real one.
/// This is needed for our testing suite since I'm opting to do more integration testing than unit testings.
/// We need certain services which require a running shell to me mocked since we won't have a shell instance in the
/// test context.
/// </summary>
public class ServiceOptions
{
    public bool MockMessenger { get; set; }
    public bool MockClipboardService { get; set; }
    public bool MockNotificationService { get; set; }
}