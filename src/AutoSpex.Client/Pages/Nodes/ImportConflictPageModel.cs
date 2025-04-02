using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Pages;

public class ImportConflictPageModel(Package package) : PageViewModel
{
    public Package Package { get; } = package;
}