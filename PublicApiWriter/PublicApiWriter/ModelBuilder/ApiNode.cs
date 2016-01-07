using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AssemblyApi.ModelBuilder
{
    internal sealed class ApiNode
    {
        private readonly ConcurrentDictionary<string, ApiNode> m_Members = new ConcurrentDictionary<string, ApiNode>();

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, long memberImportance = 0)
        {
            Signature = signature;
            Namespace = @namespace;
            SymbolAccessibility = symbolAccessibility;
            Kind = kind;
            Name = name;
            Importance = memberImportance;
        }

        /// <summary>
        /// A lower value indicates a more important member relative to its siblings
        /// </summary>
        public long Importance { get; }
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

        public ApiNode AddMember(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, long memberImportance)
        {
            return m_Members.GetOrAdd(signature, new ApiNode(signature, @namespace, symbolAccessibility, kind, name, memberImportance));
        }

        public void RemoveDescendantsWhere(Predicate<ApiNode> predicate)
        {
            foreach (var key in m_Members.Keys)
            {
                ApiNode value;
                if (m_Members.TryGetValue(key, out value) && predicate(value))
                {
                    ApiNode node;
                    m_Members.TryRemove(key, out node);
                }
            }
        }

        public override string ToString()
        {
            return $"Signature: {Signature}, Importance: {Importance}, Namespace: {Namespace}, Member count: {Members.Count()}";
        }

    }
}