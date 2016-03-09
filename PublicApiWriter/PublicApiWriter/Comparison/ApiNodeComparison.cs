using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyApi.ModelBuilder;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace AssemblyApi.Comparison
{
    internal sealed class ApiNodeComparison
    {
        public ApiNode OldApiNode { get;}
        public ApiNode NewApiNode { get;}
        public IReadOnlyCollection<ApiNodeComparison> MemberComparison{ get; }

        private ApiNodeComparison([CanBeNull] ApiNode oldApiNode, [CanBeNull] ApiNode newApiNode, IEnumerable<ApiNodeComparison> memberComparison)
        {
            OldApiNode = oldApiNode;
            NewApiNode = newApiNode;
            MemberComparison = memberComparison.ToList();
        }

        /// <returns>A list of nodes with an <see cref="ApiNodeComparison"/> for each <seealso cref="ApiNode"/> in either the <paramref name="oldApi"/> or <paramref name="newApi"/></returns>
        public static IReadOnlyCollection<ApiNodeComparison> Compare(IEnumerable<ApiNode> oldApi, IEnumerable<ApiNode> newApi)
        {
            var nodeComparisons = new List<ApiNodeComparison>();
            var newApiByName = new ConcurrentDictionary<string, ApiNode>(newApi.ToDictionary(node => node.Name, node => node));
            foreach (var oldApiNode in oldApi)
            {
                ApiNode newApiNode;
                var inNewApi = newApiByName.TryRemove(oldApiNode.Signature, out newApiNode);
                if (inNewApi)
                {
                    var memberComparison = Compare(oldApiNode.Members, newApiNode.Members);
                    nodeComparisons.Add(new ApiNodeComparison(oldApiNode, newApiNode, memberComparison));
                }
                else
                {
                    nodeComparisons.Add(new ApiNodeComparison(oldApiNode, newApiNode, Compare(oldApiNode.Members, new ApiNode[0])));
                }
            }

            nodeComparisons.AddRange(newApiByName.Values
                .Select(newApiNode => new ApiNodeComparison(null, newApiNode, Compare(new ApiNode[0], newApiNode.Members))));

            return nodeComparisons;
        }

        private string Describe(Func<ApiNode, string> func)
        {
            if (OldApiNode == null) return func(NewApiNode);
            if (NewApiNode == null) return func(OldApiNode);

            var oldDescription = func(OldApiNode);
            var newDescription = func(NewApiNode);
            return oldDescription != newDescription
                ? $"{oldDescription} -> {newDescription}"
                : oldDescription;
        }

        public string Kind => Describe(node => node.Kind.ToString());

        public string Name => Describe(node => node.Name.ToString());

        public string Namespace => Describe(node => node.Namespace.ToString());

        public string Signature => Describe(node => node.Signature.ToString());

        public string SymbolAccessibility => Describe(node => node.SymbolAccessibility.ToString());

        public override string ToString()
        {
            return $"{Name}: {SymbolAccessibility}, {Kind}";
        }

    }
}
