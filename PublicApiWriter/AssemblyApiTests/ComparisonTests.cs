﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssemblyApi.Comparison;
using AssemblyApi.ModelBuilder;
using AssemblyApi.Output;
using AssemblyApiTests.Builders;
using NUnit.Framework;
using NUnit.Framework.Internal;
using static Microsoft.CodeAnalysis.Accessibility;
using static Microsoft.CodeAnalysis.SymbolKind;


namespace AssemblyApiTests
{
    [TestFixture]
    public class ComparisonTests
    {
        [Test]
        public void ComparisonIsEqual()
        {
            var oldApi = CreateApi("1");
            var newApi = CreateApi("1");
            var comparison = Compare(oldApi, newApi);
            Assert.That(comparison, Is.Not.Empty);
        }

        private static IReadOnlyCollection<ApiNodeComparison> Compare(IApiNode oldApi, IApiNode newApi)
        {
            return ApiNodeComparison.Compare(new[] {oldApi}, new[] {newApi});
        }

        private static ApiNode CreateApi(string uniquebit)
        {
            var type = new ApiNodeBuilder(NamedType, Public, $"Type{uniquebit}");
            var namespaceNode = new ApiNodeBuilder(Namespace, Public, $"Namespace{uniquebit}").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(Assembly, Public, $"Assembly{uniquebit}").WithMembers(namespaceNode).Build();
            return assemblyNode;
        }
    }
}
