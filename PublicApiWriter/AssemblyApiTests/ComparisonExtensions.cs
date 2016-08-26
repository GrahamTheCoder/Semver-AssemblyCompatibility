using System;
using System.Collections.Generic;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApiTests.SemVer;

namespace Gtc.AssemblyApiTests
{
    internal class ComparisonExtensions
    {
        public static Versions GetNewSemanticVersion(IReadOnlyCollection<ApiNodeComparison> comparison, Version oldFileVersion)
        {
            var binaryApiCompatibility = new BinaryApiComparer().GetApiChangeType(comparison);
            return new Versions(oldFileVersion, binaryApiCompatibility);
        }
    }
}