using System;
using System.Threading;
using AutoSpex.Client.Services;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using MediatR.Pipeline;

namespace AutoSpex.Client.Behaviors;

public class ExceptionBehavior<TRequest, TResponse, TException>(IMessenger messenger) :
    IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : notnull
    where TResponse : class, IResultBase, new()
    where TException : Exception
{
    private const string Message =
        "The request encounter an unexpected error. See log for details. If this issue persists, please report it.";

    private readonly IMessenger _messenger = messenger;

    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        var error = new Error(Message).CausedBy(exception);

        /*var notification = new Notification("Request Error", error.Message, NotificationType.Error);
        _notifier.Show(notification);*/

        var response = new TResponse();
        response.Reasons.Add(error);
        
        state.SetHandled(response);
        return Task.CompletedTask;
    }
}