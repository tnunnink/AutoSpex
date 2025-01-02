using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface ICommandRequest<out TResult> : IRequest<TResult>, IChangeRequest where TResult : IResultBase;