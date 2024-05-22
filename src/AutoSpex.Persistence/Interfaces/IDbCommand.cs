using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface IDbCommand
{
    /*string ChangeMessage();*/
};

public interface IDbCommand<out TResult> : IDbCommand, IRequest<TResult> where TResult : IResultBase;