using System;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[PublicAPI]
public interface IProjectMigrator
{
    Task<Result> Migrate(Uri path);

    Result<ProjectAction> Evaluate(Uri uri);
}