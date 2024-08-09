using System.Globalization;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;

namespace AutoSpex.Client.Observers;

public class ChangeLogObserver(ChangeLog model) : Observer<ChangeLog>(model)
{
    public string Command => Model.Command;
    public string Message => Model.Message;
    public string ChangedOn => Model.ChangedOn.ToString(CultureInfo.CurrentCulture);
    public string ChangedBy => Model.ChangedBy;

    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || Command.Satisfies(filter)
               || Message.Satisfies(filter)
               || ChangedOn.Satisfies(filter)
               || ChangedBy.Satisfies(filter);
    }
}