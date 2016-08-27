using System.Collections.Generic;
using Gtc.AssemblyApi.Model;

namespace Gtc.AssemblyApi.IO
{
    internal class PrettyNodeOrderComparer : IComparer<IApiNode>
    {
        public int Compare(IApiNode x, IApiNode y)
        {
            if (x.SymbolAccessibility != y.SymbolAccessibility)
                return -x.SymbolAccessibility.CompareTo(y.SymbolAccessibility);
            if (x.Importance != y.Importance)
                return x.Importance.CompareTo(y.Importance);
            var xIsNamespace = x.Kind == SymbolKind.Namespace;
            var yIsNamespace = y.Kind == SymbolKind.Namespace;
            if (xIsNamespace != yIsNamespace)
                return xIsNamespace.CompareTo(yIsNamespace);
            return x.Name.CompareTo(y.Name);
        }
    }
}