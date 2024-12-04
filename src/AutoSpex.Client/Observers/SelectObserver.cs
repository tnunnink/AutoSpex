using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public partial class SelectObserver : StepObserver<Select>
{
    public SelectObserver(Select model) : base(model)
    {
        Property = new PropertyInput(this);
        Track(Property);
    }

    /// <summary>
    /// The <see cref="PropertyInput"/> wrapping the property value for the select step.
    /// </summary>
    public PropertyInput Property { get; }

    protected override bool PromptForDeletion => false;
}