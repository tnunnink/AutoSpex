using AutoSpex.Engine;
using AutoSpex.Persistence;
using JetBrains.Annotations;

namespace AutoSpex.Client.Messages;

[PublicAPI]
public record ProjectLaunchedMessage(Project Project);