using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.ModelBuilder;
using JetBrains.Annotations;

namespace Gtc.AssemblyApi.Comparison
{
    internal sealed class ApiNodeComparison : IApiNodeComparison
    {
        public IApiNode OldApiNode { get;}
        public IApiNode NewApiNode { get;}
        public IReadOnlyCollection<IApiNodeComparison> MemberComparison{ get; }

        public SignatureDifferenceType SignatureDifferenceType
        {
            get
            {
                return OldApiNode == null
                    ? SignatureDifferenceType.Added
                    : NewApiNode == null
                        ? SignatureDifferenceType.Removed
                        : OldApiNode.Signature == NewApiNode.Signature
                            ? SignatureDifferenceType.SignatureSame
                            : SignatureDifferenceType.SignatureEdited;
            }
        }

        private IEnumerable<IApiNodeComparison> DifferentMembers => MemberComparison.Where(n => n.IsDifferent);

        public bool IsDifferent => SignatureDifferenceType != SignatureDifferenceType.SignatureSame || DifferentMembers.Any();

        private ApiNodeComparison([CanBeNull] IApiNode oldApiNode, [CanBeNull] IApiNode newApiNode, IEnumerable<ApiNodeComparison> memberComparison)
        {
            OldApiNode = oldApiNode;
            NewApiNode = newApiNode;
            MemberComparison = memberComparison.ToList();
        }

        /// <returns>A list of nodes with an <see cref="ApiNodeComparison"/> for each <seealso cref="ApiNode"/> in either the <paramref name="oldApi"/> or <paramref name="newApi"/></returns>
        public static IReadOnlyCollection<ApiNodeComparison> Compare(IEnumerable<IApiNode> oldApi, IEnumerable<IApiNode> newApi)
        {
            var nodeComparisons = new List<ApiNodeComparison>();
            var newApiBySignature = new ConcurrentDictionary<string, IApiNode>(newApi.ToDictionary(node => node.Signature, node => node));
            foreach (var oldApiNode in oldApi)
            {
                IApiNode newApiNode;
                var inNewApi = newApiBySignature.TryRemove(oldApiNode.Signature, out newApiNode);
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

            nodeComparisons.AddRange(newApiBySignature.Values
                .Select(newApiNode => new ApiNodeComparison(null, newApiNode, Compare(new ApiNode[0], newApiNode.Members))));

            return nodeComparisons;
        }

        private string Describe(Func<IApiNode, string> func)
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

    internal enum SignatureDifferenceType
    {
        SignatureSame,
        Added,
        SignatureEdited,
        Removed
    }
}
