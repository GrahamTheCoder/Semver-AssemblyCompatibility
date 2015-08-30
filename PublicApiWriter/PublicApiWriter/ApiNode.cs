using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Linq;

namespace PublicApiWriter
{
    internal sealed class ApiNode
    {
        private readonly ConcurrentDictionary<string, ApiNode> m_Members = new ConcurrentDictionary<string, ApiNode>();

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility)
        {
            SymbolAccessibility = symbolAccessibility;
            Namespace = @namespace;
            Signature = signature;
        }

        public string Signature { get; }
        public string Namespace { get; }
        public Accessibility SymbolAccessibility { get; }
        public IEnumerable<ApiNode> Members => m_Members.Values.OrderBy(node => node.Signature).ToList();

        public ApiNode CreateRoot(string signature, string @namespace, Accessibility symbolAccessibility)
        {
            return new ApiNode(signature, @namespace, symbolAccessibility);
        }

        public ApiNode AddMember(string signature, string @namespace, Accessibility symbolAccessibility)
        {
            return m_Members.GetOrAdd(signature, new ApiNode(signature, @namespace, symbolAccessibility));
        }
        public bool Contains(ApiNode member)
        {
            return member.Contains(member);
        }
        
        public override string ToString()
        {
            return $"Signature: {Signature}, Namespace: {Namespace}, Member count: {Members.Count()}";
        }

    }
}