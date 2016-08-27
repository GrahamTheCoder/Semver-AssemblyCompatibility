using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.SemVer;

namespace Gtc.AssemblyApi.Extensions
{
    internal static class ComparisonExtensions
    {
        public static Versions GetNewSemanticVersion(this IEnumerable<ApiNodeComparison> comparison, Version oldFileVersion)
        {
            var binaryApiCompatibility = new BinaryApiComparer().GetApiChangeType(comparison);
            return new Versions(oldFileVersion, binaryApiCompatibility);
        }

        public static IEnumerable<IApiNodeComparison> GetDifferences(this IEnumerable<ApiNodeComparison> comparison)
        {
            return comparison.Select(n => new DifferentOnlyApiNodeComparison(n));
        }

        public static async Task<string> GetDifferencesString(this IEnumerable<ApiNodeComparison> comparison)
        {
            using (var sw = new StringWriter())
            {
                await ApiComparisonWriter.Write(comparison.GetDifferences(), sw);
                return sw.ToString();
            }
        }
    }
}