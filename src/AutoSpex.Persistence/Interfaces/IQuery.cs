using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface IQuery;
public interface IQuery<out TResult> : IQuery, IRequest<TResult> where TResult : IResultBase;