using System.Collections.ObjectModel;
using L5Spex.Client.Common;

namespace L5Spex.Client.ViewModels;

internal class SpecTreeViewModelDesign : SpecTreeViewModel
{
    public SpecTreeViewModelDesign() : base(null!)
    {
        Projects = new ObservableCollection<Observers.NodeObserver>
        {
            new("My Project", NodeType.Project)
            {
                Nodes = new ObservableCollection<Observers.NodeObserver>
                {
                    new("My Project/Folder Name", NodeType.Folder)
                    {
                        Nodes = new ObservableCollection<Observers.NodeObserver>
                        {
                            new("My Project/Folder Name/Specification#1", NodeType.Specification),
                            new("My Project/Folder Name/Specification#2", NodeType.Specification),
                            new("My Project/Folder Name/Specification#3", NodeType.Specification),
                            new("My Project/Folder Name/Specification#4", NodeType.Specification)
                        }
                    },
                    new("My Project/Specification", NodeType.Specification),
                    new("My Project/Specification", NodeType.Specification),
                    new("My Project/Specification", NodeType.Specification),
                    new("My Project/AlarmCheck", NodeType.Folder)
                    {
                        Nodes = new ObservableCollection<Observers.NodeObserver>
                        {
                            new("My Project/AlarmCheck/Another Spec", NodeType.Specification),
                            new("My Project/AlarmCheck/Tag Spec", NodeType.Specification),
                            new("My Project/AlarmCheck/tag with name Has correct alarm set point Value",
                                NodeType.Specification)
                        }
                    },
                }
            }
        };
    }
}