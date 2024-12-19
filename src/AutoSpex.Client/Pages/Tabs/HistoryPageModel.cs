using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class HistoryPageModel(Observer observer) : PageViewModel("History")
{
    public override string Route => $"{observer.Entity}/{observer.Id}/{Title}";
    public override string Icon => "IconLineClockRotate";
}