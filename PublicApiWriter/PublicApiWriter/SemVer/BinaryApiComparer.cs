using System;
using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.Comparison;

namespace Gtc.AssemblyApi.SemVer
{
    internal class BinaryApiComparer
    {
        public BinaryApiCompatibility GetApiChangeType(IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            switch (HighestDifferenceLevel(comparison))
            {
                case SignatureDifferenceType.SignatureSame:
                    return BinaryApiCompatibility.Identical;
                case SignatureDifferenceType.Added:
                    return BinaryApiCompatibility.BackwardsCompatible;
                case SignatureDifferenceType.SignatureEdited:
                case SignatureDifferenceType.Removed:
                    return BinaryApiCompatibility.Incompatible;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison));
            }
        }

        private SignatureDifferenceType HighestDifferenceLevel(IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            return comparison.Max(n => HighestDifferenceLevel((IApiNodeComparison) n));
        }

        private static SignatureDifferenceType HighestDifferenceLevel(IApiNodeComparison nodeComparison)
        {
            return
                nodeComparison.MemberComparison.Select(HighestDifferenceLevel)
                    .Concat(new[] {nodeComparison.SignatureDifferenceType})
                    .Max();
        }

        public IEnumerable<IApiNodeComparison> GetDifferences(IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            return comparison.Select(n => new DifferentOnlyApiNodeComparison(n));
        }
    }
}