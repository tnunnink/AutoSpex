using System.Threading.Tasks;
using Avalonia.Input;

namespace AutoSpex.Client.Services;

public interface IClipboardService
{
    Task ClearAsync();

    Task<object?> GetDataAsync(string format);
    
    Task<string?> GetTextAsync();
    
    Task SetDataAsync(IDataObject data);
    
    Task SetTextAsync(string text);
}