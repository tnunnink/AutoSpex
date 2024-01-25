using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public record CommandNotification<TResult>(IDbCommand<TResult> Command, string Name, IResultBase Result)
    : INotification where TResult : IResultBase;