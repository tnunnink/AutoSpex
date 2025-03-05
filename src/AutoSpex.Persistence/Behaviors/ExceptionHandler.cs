using FluentResults;
using MediatR.Pipeline;
using NLog;

namespace AutoSpex.Persistence;

public class ExceptionBehavior<TRequest, TResponse, TException> :
    IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : notnull
    where TResponse : class, IResultBase, new()
    where TException : Exception
{
    
    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        var error = new Error("Request failed").CausedBy(exception);
        //Logger.Error(error);
        
        var response = new TResponse();
        response.Reasons.Add(error);
        state.SetHandled(response);
        
        return Task.CompletedTask;
    }
}