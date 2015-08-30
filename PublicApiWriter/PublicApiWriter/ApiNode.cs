using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Linq;

namespace PublicApiWriter
{
    internal sealed class ApiNode
    {
        private readonly ConcurrentDictionary<string, ApiNode> m_Members = new ConcurrentDictionary<string, ApiNode>();

        public ApiNode(string signature, string name, string @namespace, Accessibility symbolAccessibility, int memberImportance = 0)
        {
            SymbolAccessibility = symbolAccessibility;
            Namespace = @namespace;
            Signature = signature;
            Name = name;
            Importance = memberImportance;
        }

        /// <summary>
        /// A higher value indicates a more important member relative to its siblings
        /// </summary>
        public int Importance { get; }
        public string Name { get; }
        public string Namespace { get; }
        public string Signature { get; }
        public Accessibility SymbolAccessibility { get; }
        public IEnumerable<ApiNode> Members => m_Members.Values.ToList();

        public static ApiNode CreateAssemblyRoot(string assemblyName)
        {
            return new ApiNode("assembly " + assemblyName, assemblyName, assemblyName, Accessibility.Public);
        }

        public ApiNode AddMember(string signature, string name, string @namespace, Accessibility symbolAccessibility, int memberImportance)
        {
            return m_Members.GetOrAdd(signature, new ApiNode(signature, name, @namespace, symbolAccessibility, memberImportance));
        }

        public bool Contains(ApiNode member)
        {
            return member.Contains(member);
        }

        public override string ToString()
        {
            return $"Signature: {Signature}, Importance: {Importance}, Namespace: {Namespace}, Member count: {Members.Count()}";
        }

    }
}