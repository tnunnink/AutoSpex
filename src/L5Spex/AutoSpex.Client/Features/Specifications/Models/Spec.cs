using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Specifications;

public partial class Spec : ObservableObject
{
    [ObservableProperty] private Guid _nodeId;
}