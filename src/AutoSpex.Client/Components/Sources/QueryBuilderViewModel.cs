using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Components.Sources;

public partial class QueryBuilderViewModel : PageViewModel
{
    
    public static IEnumerable<Element> Components => Element.List.Where(e => e.IsComponent).OrderBy(e => e.Name);

    public static IEnumerable<Element> Elements => Element.List.Where(e => !e.IsComponent).OrderBy(e => e.Name);
    
}