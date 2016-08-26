using System;
using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.Comparison;

namespace Gtc.AssemblyApi.SemVer
{
    internal static class ComparisonExtensions
    {
        public static Versions GetNewSemanticVersion(this IReadOnlyCollection<ApiNodeComparison> comparison, Version oldFileVersion)
        {
            var binaryApiCompatibility = new BinaryApiComparer().GetApiChangeType(comparison);
            return new Versions(oldFileVersion, binaryApiCompatibility);
        }

        public static IEnumerable<IApiNodeComparison> GetDifferences(this IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            return comparison.Select(n => new DifferentOnlyApiNodeComparison(n));
        }
    }
}