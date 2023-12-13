using System;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Services;

public interface ISettingsManager : IDisposable, IAsyncDisposable
{
    string Get(Setting setting);
    
    T Get<T>(Setting setting, Func<string, T> convert);
    
    string? Find(Setting setting);
    
    T? Find<T>(Setting setting, Func<string, T> convert);

    void Set(Setting setting, string value);
    
    void Set<T>(Setting setting, T value);

    Task Load();

    Task Save();
}