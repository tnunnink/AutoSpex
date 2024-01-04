using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace AutoSpex.Client.Services;

public class ClipboardService : IClipboardService
{
    private readonly IClipboard _clipboard;

    public ClipboardService(TopLevel host)
    {
        _clipboard = host.Clipboard ?? throw new InvalidOperationException("Somehow the clipboard is null.");
    }
    
    public Task ClearAsync()
    {
        return _clipboard.ClearAsync();
    }

    public Task<object?> GetDataAsync(string format)
    {
        return _clipboard.GetDataAsync(format);
    }

    public Task<string?> GetTextAsync()
    {
        return _clipboard.GetTextAsync();
    }

    public Task SetDataAsync(IDataObject data)
    {
        return _clipboard.SetDataObjectAsync(data);
    }

    public Task SetTextAsync(string text)
    {
        return _clipboard.SetTextAsync(text);
    }
}