using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface IDbQuery;
public interface IDbQuery<out TResult> : IDbQuery, IRequest<TResult> where TResult : IResultBase;