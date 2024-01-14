using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features;

[PublicAPI]
public record ProjectLaunchedMessage(Project Project);