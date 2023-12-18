using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record ProjectLaunchedMessage(Project Project);