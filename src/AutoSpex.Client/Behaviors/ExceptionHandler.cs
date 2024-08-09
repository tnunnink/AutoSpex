using System;
using System.Threading;
using AutoSpex.Client.Services;
using FluentResults;
using MediatR.Pipeline;

namespace AutoSpex.Client.Behaviors;

public class ExceptionBehavior<TRequest, TResponse, TException>(Notifier notifier) :
    IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : notnull
    where TResponse : class, IResultBase, new()
    where TException : Exception
{
    private const string Message =
        "The request encounter an unexpected error. See log for details. If this issue persists, please report it.";

    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        var error = new Error(Message).CausedBy(exception);

        notifier.NotifyError("Request Error", error.Message);

        var response = new TResponse();
        response.Reasons.Add(error);

        state.SetHandled(response);
        return Task.CompletedTask;
    }
}