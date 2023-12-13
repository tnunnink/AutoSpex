using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record LaunchProjectMessage(Project Project);