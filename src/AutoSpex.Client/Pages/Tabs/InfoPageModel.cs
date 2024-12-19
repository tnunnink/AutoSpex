using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class InfoPageModel(Observer observer) : PageViewModel("Details")
{
    public override string Route => $"{observer.Entity}/{observer.Id}/{Title}";
    public override string Icon => "IconLineInfo";
}