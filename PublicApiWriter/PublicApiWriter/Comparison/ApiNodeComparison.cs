using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtc.AssemblyApi.Model;
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
                        : SignatureDifferenceType.SignatureSame;
            }
        }

        private IEnumerable<IApiNodeComparison> DifferentMembers => MemberComparison.Where(n => n.IsDifferent);

        public bool IsDifferent => SignatureDifferenceType != SignatureDifferenceType.SignatureSame || DifferentMembers.Any();
        public string Name => Get(n => n.Name);
        public string Signature => Get(n => n.Signature);

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


        public override string ToString()
        {
            return DescribeChanges(MemberComparison);
        }

        private string DescribeChanges(IReadOnlyCollection<IApiNodeComparison> members)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{ChangeTypeIndicator()} {Signature}");
            foreach (var apiNodeComparison in members)
            {
                sb.AppendLine(apiNodeComparison.ToString());
            }
            return sb.ToString();
        }

        private string ChangeTypeIndicator()
        {
            switch (SignatureDifferenceType)
            {
                case SignatureDifferenceType.SignatureSame:
                    return " ";
                case SignatureDifferenceType.Added:
                    return "+";
                case SignatureDifferenceType.Removed:
                    return "-";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public T Get<T>(Func<IApiNode, T> describe)
        {
            switch (SignatureDifferenceType)
            {
                case SignatureDifferenceType.SignatureSame:
                case SignatureDifferenceType.Added:
                    return describe(NewApiNode);
                case SignatureDifferenceType.Removed:
                    return describe(OldApiNode);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
