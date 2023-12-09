using System;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Services;

public interface ISettingsManager : IDisposable, IAsyncDisposable
{
    string Get(Setting setting);
    
    T Get<T>(Setting setting, Func<string, T> convert);
    
    void Save(Setting setting, string value);
}