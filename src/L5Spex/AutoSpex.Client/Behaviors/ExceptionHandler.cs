using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using FluentResults;
using MediatR.Pipeline;

namespace AutoSpex.Client.Behaviors;

public class ExceptionBehavior<TRequest, TResponse, TException> :
    IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : notnull
    where TResponse : class, IResultBase, new()
    where TException : Exception
{
    public ExceptionBehavior()
    {
        //notification manager to send notification to UI for failure.
    }
    
    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        var error = new Error("This request encountered an unexpected error. If this issue persists, please report it.")
            .CausedBy(exception);

        /*await UserPromptBuilder.Configure()
            .WithHeaderContent("This is embarrassing...")
            .WithContent(error.Message)
            .WithExpandedInformation("Error Message", "Error Message",
                error.Reasons.First(r => r is ExceptionalError).Message)
            .WithStandardButtons(MessageBoxButtons.OK)
            .WithStatusImage(MessageBoxImage.Error)
            .Show();*/

        var response = new TResponse();
        response.Reasons.Add(error);
        state.SetHandled(response);
        return Task.CompletedTask;
    }
}