using L5Sharp;
using L5Spex.Engine.Enumerations;

namespace L5Spex.Engine.Contracts;

public interface ISpecification
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="config"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<RunResult> Run(LogixContent content, RunConfig? config = default, CancellationToken token = default);
}