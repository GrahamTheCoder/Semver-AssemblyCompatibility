using System;
using System.Collections.Generic;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.ModelBuilder;
using Gtc.AssemblyApi.SemVer;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    [TestFixture]
    public class ComparisonTests
    {
        private readonly BinaryApiComparer m_BinaryApiComparer = new BinaryApiComparer();

        [Test]
        public void IdenticalAssembliesAreEqual()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("1");
            var comparison = Compare(oldApi, newApi);
            Assert.That(comparison.GetDifferences(),
                Has.All.Matches<IApiNodeComparison>(n => !n.IsDifferent));
        }

        [Test]
        public void TotallyDifferentAssembliesAreNotEqual()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(comparison.GetDifferences(),
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

            Assert.That(comparison.GetNewSemanticVersion(new Version(1,0,0,0)).AssemblyFileVersion.Major, Is.EqualTo(2));
        }


        private static IReadOnlyCollection<ApiNodeComparison> Compare(IApiNode oldApi, IApiNode newApi)
        {
            return ApiNodeComparison.Compare(new[] {oldApi}, new[] {newApi});
        }
    }
}
