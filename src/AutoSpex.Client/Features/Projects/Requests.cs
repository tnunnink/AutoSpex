using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[PublicAPI]
public static class ProjectRequest
{
    public record Create(Project Project) : IRequest<Result>;

    public record CopyPath(Project Project) : IRequest;
    
    public record LocateProject(Project Project) : IRequest<Result>;
}