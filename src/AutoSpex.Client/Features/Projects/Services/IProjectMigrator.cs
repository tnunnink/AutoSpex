using System;
using System.Threading.Tasks;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects.Services;

[PublicAPI]
public interface IProjectMigrator
{
    Task<Result> Migrate(Uri path);

    Result<ProjectAction> Evaluate(Uri uri);
}