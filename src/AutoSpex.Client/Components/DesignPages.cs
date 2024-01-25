using System.Collections.ObjectModel;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Components;

public static class DesignPages
{
    public static ObservableCollection<PageViewModel> Pages =
    [
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
    ];
}