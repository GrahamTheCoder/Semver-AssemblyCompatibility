using System.Collections.Generic;
using AssemblyApi.Comparison;
using AssemblyApi.ModelBuilder;
using NUnit.Framework;


namespace AssemblyApiTests
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
        public void SemVerIncompatible()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(m_BinaryApiComparer.GetApiChangeType(comparison), Is.EqualTo(BinaryApiCompatibility.Incompatible));
        }


        private static IReadOnlyCollection<ApiNodeComparison> Compare(IApiNode oldApi, IApiNode newApi)
        {
            return ApiNodeComparison.Compare(new[] {oldApi}, new[] {newApi});
        }
    }
}
