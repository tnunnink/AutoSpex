using System;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;

namespace AutoSpex.Client.Observers;

public class ReferenceObserver(Reference model) : Observer<Reference>(model)
{
    private readonly VariableObserver? _variable;

    /// <summary>
    /// Creates a <see cref="ReferenceObserver"/> given an existing <see cref="VariableObserver"/> object.
    /// </summary>
    /// <param name="variable">The variable to create the reference for.</param>
    public ReferenceObserver(VariableObserver variable) : this(variable.Model.Reference())
    {
        _variable = variable;
    }

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
        //If created from a variable in memory, we would have the reference to use, so return that.
        if (_variable is not null) return _variable;

        //If this was loaded as part of a spec argument, we should be able to resolve it through the database.
        var result = await Mediator.Send(new GetReferenceVariable(Model));
        return result.IsSuccess ? new VariableObserver(result.Value) : default;
    }
}