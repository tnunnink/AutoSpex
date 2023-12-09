using L5Sharp.Core;

namespace AutoSpex.Engine.Contracts;

public interface ISpecification
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="config"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<RunResult> Run(L5X file, RunConfig? config = default, CancellationToken token = default);
}