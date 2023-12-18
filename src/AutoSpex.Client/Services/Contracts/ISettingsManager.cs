using System;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Services;

public interface ISettingsManager : IDisposable, IAsyncDisposable
{
    void Add<T>(Setting setting, T value);
    
    string Get(Setting setting);
    
    T Get<T>(Setting setting, Func<string, T> convert);
    
    string? Find(Setting setting);
    
    T? Find<T>(Setting setting, Func<string, T> convert);
    
    void Set<T>(Setting setting, T value);

    Task Save();
}