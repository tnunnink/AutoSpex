using System.Threading;

namespace AutoSpex.Client.Messages;

public record ProcessMessage(string Process, bool IsActive, CancellationTokenSource Cancellation);