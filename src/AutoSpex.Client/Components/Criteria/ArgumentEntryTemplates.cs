﻿using System.Collections.Generic;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class ArgumentEntryTemplates : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        if (param is not ArgumentObserver argument) return default;

        if (argument.Value is Criterion)
        {
            return Templates[nameof(Criterion)].Build(param);
        }

        if (argument.Criterion is not null && argument.Criterion.Operation == Operation.In)
        {
            return Templates[nameof(Operation.In)].Build(param);
        }

        return Templates["Default"].Build(param);
    }

    public bool Match(object? data) => data is ArgumentObserver;
}