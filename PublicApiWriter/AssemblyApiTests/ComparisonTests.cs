using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssemblyApi.Comparison;
using AssemblyApi.ModelBuilder;
using AssemblyApi.Output;
using AssemblyApiTests.Builders;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using static Microsoft.CodeAnalysis.Accessibility;
using static Microsoft.CodeAnalysis.SymbolKind;


namespace AssemblyApiTests
{
    [TestFixture]
    public class ComparisonTests
    {
        [Test]
        public void IdenticalAssembliesAreEqual()
        {
            var oldApi = CreateApi("1");
            var newApi = CreateApi("1");
            var comparison = Compare(oldApi, newApi);
            Assert.That(GetDifferences(comparison),
                Has.All.Matches<IApiNodeComparison>(n => !n.IsDifferent));
        }

        [Test]
        public void TotallyDifferentAssembliesAreNotEqual()
        {
            var oldApi = CreateApi("1");
            var newApi = CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(GetDifferences(comparison),
                Has.All.Matches<IApiNodeComparison>(n => n.IsDifferent));
        }

        [Test]
        public void BinaryIncompatible()
        {
            var oldApi = CreateApi("1");
            var newApi = CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(GetApiChangeType(comparison), Is.EqualTo(BinaryApiCompatibility.Incompatible));
        }

        [Test]
        public void SemVerIncompatible()
        {
            var oldApi = CreateApi("1");
            var newApi = CreateApi("2");
            var comparison = Compare(oldApi, newApi);
            Assert.That(GetApiChangeType(comparison), Is.EqualTo(BinaryApiCompatibility.Incompatible));
        }

        private BinaryApiCompatibility GetApiChangeType(IReadOnlyCollection<ApiNodeComparison> comparison)
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
            return comparison.Max(n => HighestDifferenceLevel(n));
        }

        private static SignatureDifferenceType HighestDifferenceLevel(IApiNodeComparison nodeComparison)
        {
            return
                nodeComparison.MemberComparison.Select(HighestDifferenceLevel)
                    .Concat(new[] {nodeComparison.SignatureDifferenceType})
                    .Max();
        }

        private IEnumerable<IApiNodeComparison> GetDifferences(IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            return comparison.Select(n => new DifferentOnlyApiNodeComparison(n));
        }

        private static IReadOnlyCollection<ApiNodeComparison> Compare(IApiNode oldApi, IApiNode newApi)
        {
            return ApiNodeComparison.Compare(new[] {oldApi}, new[] {newApi});
        }

        private static ApiNode CreateApi(string uniquebit)
        {
            var type = new ApiNodeBuilder(NamedType, Public, $"Type{uniquebit}");
            var namespaceNode = new ApiNodeBuilder(Namespace, Public, $"Namespace{uniquebit}").WithMembers(type);
            var assemblyNode =
                new ApiNodeBuilder(SymbolKind.Assembly, Public, $"Assembly{uniquebit}").WithMembers(namespaceNode).Build();
            return assemblyNode;
        }
    }
}
