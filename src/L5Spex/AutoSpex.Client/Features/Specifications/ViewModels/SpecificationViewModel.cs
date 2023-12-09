using System;
using System.Threading.Tasks;
using Node = AutoSpex.Client.Features.Nodes.Node;
using NodeViewModel = AutoSpex.Client.Features.Nodes.NodeViewModel;

namespace AutoSpex.Client.Features.Specifications;

public partial class SpecificationViewModel : Nodes.NodeViewModel
{
    public SpecificationViewModel(Node node) : base(node, new SpecificationView())
    {
    }

    protected override Task Save()
    {
        throw new NotImplementedException();
    }
}