using AssemblyApi.ModelBuilder;
using AssemblyApi.Output;
using AssemblyApiTests.Builders;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Microsoft.CodeAnalysis.Accessibility;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace AssemblyApiTests
{
    [TestFixture]
    public class ApiFilterTests
    {
        [TestCase(Public, @"SomeOtherNamespace", 1)]
        [TestCase(Private, @"SomeOtherNamespace", 0)]
        [TestCase(Public, @"Api\.Tests", 0)]
        public void NamespaceOnlyExcludedWhenContentsAreExcluded(Accessibility typeAccessibility, string namespaceExclusionRegex, int expectedMembers)
        {
            var type = new ApiNodeBuilder(NamedType, typeAccessibility);
            var namespaceNode = new ApiNodeBuilder(Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(Assembly).WithMembers(namespaceNode).Build();
            var apiFilter = new ApiFilter(new PrinterConfig("", namespaceExclusionRegex));

            apiFilter.ApplyTo(new[] { assemblyNode });

            Assert.That(assemblyNode.Members, Has.Count.EqualTo(expectedMembers));
        }
    }
}
