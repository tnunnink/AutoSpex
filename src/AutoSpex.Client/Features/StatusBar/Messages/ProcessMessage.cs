using System.Threading;

namespace AutoSpex.Client.Features.StatusBar;

public record ProcessMessage(string Process, bool IsActive, CancellationTokenSource Cancellation);