using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface IDbCommand;

public interface IDbCommand<out TResult> : IDbCommand, IRequest<TResult> where TResult : IResultBase;