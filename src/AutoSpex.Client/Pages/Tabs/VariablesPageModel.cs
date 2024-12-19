using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class VariablesPageModel(Observer observer) : PageViewModel("Variables")
{
    public override string Route => $"{observer.Entity}/{observer.Id}/{Title}";
    public override string Icon => "IconLineAt";
}