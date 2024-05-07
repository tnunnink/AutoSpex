using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public class ElementObserver(LogixElement model) : Observer<LogixElement>(model)
{
    public Element Element => Element.FromName(Model.GetType().Name);
    public IEnumerable<PropertyObserver> Properties => Element.Properties.Select(p => new PropertyObserver(p, this));

    public override string? ToString()
    {
        return Model switch
        {
            DataTypeMember member => member.Name,
            Parameter parameter => parameter.Name,
            Tag tag => tag.TagName,
            LogixCode code => $"{code.Container} > {code.Routine?.Name} > {code.Location}",
            LogixComponent component => component.Name,
            _ => Model.ToString()
        };
    }
}