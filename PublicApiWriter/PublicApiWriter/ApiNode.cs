using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Linq;

namespace PublicApiWriter
{
    internal sealed class ApiNode
    {
        private readonly ConcurrentDictionary<string, ApiNode> m_Members = new ConcurrentDictionary<string, ApiNode>();

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, int memberImportance = 0)
        {
            Signature = signature;
            Namespace = @namespace;
            SymbolAccessibility = symbolAccessibility;
            Kind = kind;
            Name = name;
            Importance = memberImportance;
        }

        /// <summary>
        /// A higher value indicates a more important member relative to its siblings
        /// </summary>
        public int Importance { get; }
        public SymbolKind Kind { get; }
        public string Name { get; }
        public string Namespace { get; }
        public string Signature { get; }
        public Accessibility SymbolAccessibility { get; }
        public IEnumerable<ApiNode> Members => m_Members.Values.ToList();


        public static ApiNode CreateAssemblyRoot(string assemblyName)
        {
            return new ApiNode("assembly " + assemblyName, assemblyName, Accessibility.Public, SymbolKind.Assembly, assemblyName);
        }

        public ApiNode AddMember(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, int memberImportance)
        {
            return m_Members.GetOrAdd(signature, new ApiNode(signature, @namespace, symbolAccessibility, kind, name, memberImportance));
        }

        public override string ToString()
        {
            return $"Signature: {Signature}, Importance: {Importance}, Namespace: {Namespace}, Member count: {Members.Count()}";
        }

    }
}