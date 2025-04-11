using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class NavigationPageModel : PageViewModel
{
    public Task<NodeTreePageModel> NodeTree => Navigator.Navigate<NodeTreePageModel>();
}