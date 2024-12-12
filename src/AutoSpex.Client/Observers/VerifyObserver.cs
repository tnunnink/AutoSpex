using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class VerifyObserver : StepObserver<Verify>
{
    public VerifyObserver(Verify model) : base(model)
    {
    }
}