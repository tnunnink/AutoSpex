using System.Threading.Tasks;
using Avalonia.Controls;

namespace AutoSpex.Client.Services;

public interface IDialogService
{
    Task Show(Control dialog, string? title = default);
    
    Task<TResult> Show<TResult>(Control dialog, string? title = default);
}