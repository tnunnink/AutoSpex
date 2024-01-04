using System.Threading.Tasks;
using Avalonia.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services.Mocks;

[UsedImplicitly]
public class MockClipboardService : IClipboardService
{
    public Task ClearAsync()
    {
        return Task.CompletedTask;
    }

    public Task<object?> GetDataAsync(string format)
    {
        return Task.FromResult<object?>(default);
    }

    public Task<string?> GetTextAsync()
    {
        return Task.FromResult<string?>(default);
    }

    public Task SetDataAsync(IDataObject data)
    {
        return Task.CompletedTask;
    }

    public Task SetTextAsync(string text)
    {
        return Task.CompletedTask;
    }
}