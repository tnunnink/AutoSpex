using System;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;

namespace AutoSpex.Client.Observers;

public class ReferenceObserver(Reference model) : Observer<Reference>(model)
{
    /// <inheritdoc />
    public override Guid Id => Model.ReferenceId;

    /// <inheritdoc />
    public override string Name => Model.Name;

    /// <summary>
    /// The variable observer instance this reference resolves to. We want this here for a popup/tooltip display.
    /// If null then we can use that information to alert the user this is unresolvable. 
    /// </summary>
    public Task<VariableObserver?> Variable => ResolveReferenceVariable();

    /// <inheritdoc />
    public override string ToString() => Model.ToString();

    /// <summary>
    /// Retrieves the referenced variable that is in the scope of where this reference is defined.
    /// We relay on the database to figure this out.
    /// </summary>
    private async Task<VariableObserver?> ResolveReferenceVariable()
    {
        var result = await Mediator.Send(new GetReferenceVariable(Model));
        return result.IsSuccess ? new VariableObserver(result.Value) : default;
    }
}