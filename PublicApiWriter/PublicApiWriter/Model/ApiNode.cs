using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.Extensions;
using Gtc.AssemblyApi.Model;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Gtc.AssemblyApi.ModelBuilder
{
    internal sealed class ApiNode : IApiNode
    {
        private readonly ConcurrentDictionary<string, ApiNode> m_Members = new ConcurrentDictionary<string, ApiNode>();

        [JsonConstructor, UsedImplicitly]
        private ApiNode(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind,
            string name, Dictionary<string, List<string>> attributes, long importance,
            IEnumerable<ApiNode> members)
            :this (signature, @namespace, symbolAccessibility, kind, name, attributes, importance)
        {
            foreach (var member in members)
            {
                AddMember(member);
            }
        }

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, Dictionary<string, List<string>> attributes = null, long importance = 0)
        {
            Attributes = attributes ?? new Dictionary<string, List<string>>();
            Signature = signature;
            Namespace = @namespace;
            SymbolAccessibility = symbolAccessibility;
            Kind = kind;
            Name = name;
            Importance = importance;
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

        public Dictionary<string, List<string>> Attributes { get; }

        public IEnumerable<IApiNode> Members => m_Members.Values.ToList();


        internal static ApiNode CreateAssemblyRoot(string assemblyName)
        {
            return new ApiNode("assembly " + assemblyName, assemblyName, Accessibility.Public, SymbolKind.Assembly, assemblyName);
        }

        internal ApiNode AddMember(string signature, string @namespace, Accessibility symbolAccessibility, SymbolKind kind, string name, ILookup<string, string> attributes = null, long memberImportance = 0)
        {
            attributes = attributes ?? CreateEmptyLookup<string>();
            var apiNode = new ApiNode(signature, @namespace, symbolAccessibility, kind, name, attributes.ToDictionary(), memberImportance);
            return AddMember(apiNode);
        }

        private ApiNode AddMember(ApiNode apiNode)
        {
            return m_Members.GetOrAdd(apiNode.Signature, apiNode);
        }

        private static ILookup<T, T> CreateEmptyLookup<T>()
        {
            return new T[0].ToLookup(_ => _, _ => _);
        }

        public override string ToString()
        {
            return $"Signature: {Signature}, Importance: {Importance}, Namespace: {Namespace}, Member count: {Members.Count()}";
        }
    }
}