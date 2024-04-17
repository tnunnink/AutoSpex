using AutoSpex.Engine;
using FluentValidation;

namespace AutoSpex.Persistence.Variables;

public class VariableValidator : AbstractValidator<Variable>
{
    public VariableValidator()
    {
        RuleFor(x => x.VariableId).NotEmpty();
        RuleFor(x => x.NodeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
    }
}