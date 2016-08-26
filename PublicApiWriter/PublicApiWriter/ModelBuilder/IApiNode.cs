using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Gtc.AssemblyApi.ModelBuilder
{
    internal interface IApiNode
    {
        /// <summary>
        /// A lower value indicates a more important member relative to its siblings
        /// </summary>
        long Importance { get; }

        SymbolKind Kind { get; }
        string Name { get; }
        string Namespace { get; }
        string Signature { get; }
        Accessibility SymbolAccessibility { get; }
        Dictionary<string, List<string>> Attributes { get; }
        IEnumerable<IApiNode> Members { get; }
        string ToString();
    }
}