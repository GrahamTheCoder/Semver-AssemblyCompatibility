using System;
using System.Collections.Generic;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.ModelBuilder;
using Gtc.AssemblyApiTests.SemVer;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    [TestFixture]
    public class ComparisonTests
    {
        private readonly BinaryApiComparer m_BinaryApiComparer = new BinaryApiComparer();
        private readonly ComparisonExtensions m_ComparisonExtensions = new ComparisonExtensions();

        [Test]
        public void IdenticalAssembliesAreEqual()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("1");
            var comparison = Compare(oldApi, newApi);
            Assert.That(m_BinaryApiComparer.GetDifferences(comparison),
                Has.All.Matches<IApiNodeComparison>(n => !n.IsDifferent));
        }

        [Test]
        public void TotallyDifferentAssembliesAreNotEqual()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(m_BinaryApiComparer.GetDifferences(comparison),
                Has.All.Matches<IApiNodeComparison>(n => n.IsDifferent));
        }

        [Test]
        public void BinaryIncompatible()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(m_BinaryApiComparer.GetApiChangeType(comparison), Is.EqualTo(BinaryApiCompatibility.Incompatible));
        }

        [Test]
        public void GivenIncompatibleApiThenMajorVersionIncreases()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            
            Assert.That(ComparisonExtensions.GetNewSemanticVersion(comparison, new Version(1,0,0,0)).AssemblyFileVersion.Major, Is.EqualTo(2));
        }


        private static IReadOnlyCollection<ApiNodeComparison> Compare(IApiNode oldApi, IApiNode newApi)
        {
            return ApiNodeComparison.Compare(new[] {oldApi}, new[] {newApi});
        }
    }
}
