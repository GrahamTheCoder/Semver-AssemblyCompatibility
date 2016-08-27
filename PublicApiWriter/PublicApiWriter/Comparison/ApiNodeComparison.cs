using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.Model;
using JetBrains.Annotations;

namespace Gtc.AssemblyApi.Comparison
{
    internal abstract class ApiNodeComparisonBase
    {
        public abstract IApiNode OldApiNode { get; }
        public abstract IApiNode NewApiNode { get; }
        public abstract SignatureDifferenceType SignatureDifferenceType { get; }

        protected async Task<string> DescribeChanges(IReadOnlyCollection<IApiNodeComparison> members)
        {
            using (var stringWriter = new StringWriter())
            {
                await ApiComparisonWriter.Write(members, stringWriter);
                return stringWriter.ToString();
            }
        }

        protected T GetMostRecent<T>(Func<IApiNode, T> describe)
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

    internal sealed class ApiNodeComparison : ApiNodeComparisonBase, IApiNodeComparison
    {
        public override IApiNode OldApiNode { get;}
        public override IApiNode NewApiNode { get;}
        public IReadOnlyCollection<IApiNodeComparison> MemberComparison{ get; }

        public override SignatureDifferenceType SignatureDifferenceType
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
            return DescribeChanges(MemberComparison).Result;
        }

        public T Get<T>(Func<IApiNode, T> describe)
        {
            return GetMostRecent(describe);
        }
    }
}
