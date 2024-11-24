using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class SelectObserver : StepObserver<Select>
{
    public SelectObserver(Select model) : base(model)
    {
    }
}