using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public interface ICommand;

public interface ICommand<out TResult> : ICommand, IRequest<TResult> where TResult : IResultBase;