using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class CommentsPageModel(Observer observer) : PageViewModel("Comments")
{
    public override string Route => $"{observer.Icon}/{observer.Id}/{Title}";
    public override string Icon => "IconLineComment";
}