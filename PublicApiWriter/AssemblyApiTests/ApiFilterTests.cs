using System.Collections.Generic;
using System.Linq;
using AssemblyApi.IO;
using AssemblyApi.ModelBuilder;
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
            var filtered = GetFiltered(new PrinterConfig("", namespaceExclusionRegex), assemblyNode);

            Assert.That(filtered.Members.Count(), Is.EqualTo(expectedMembers));
        }

        [Test]
        public void IncludedIfMatchedByInclude()
        {
            var toIncludeRegex = @"Api\.Tests";
            var type = new ApiNodeBuilder(NamedType, Public);
            var namespaceNode = new ApiNodeBuilder(Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(Assembly).WithMembers(namespaceNode).Build();
            var filtered = GetFiltered(new PrinterConfig(toIncludeRegex, ""), assemblyNode);

            Assert.That(filtered.Members, Is.Not.Empty);
        }

        [Test]
        public void AllExcludedIfDoNotMatchInclude()
        {
            var toInclude = "Api.NonExistent";
            var type = new ApiNodeBuilder(NamedType, Public);
            var namespaceNode = new ApiNodeBuilder(Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(Assembly).WithMembers(namespaceNode).Build();

            var filtered = GetFiltered(new PrinterConfig(toInclude, ""), assemblyNode);
            Assert.That(filtered.Members, Is.Empty);
        }

        [Test]
        public void ExcludeTakesPrecedenceOverInclude()
        {
            var namespaceRegex = @"Api\.Tests";
            var type = new ApiNodeBuilder(NamedType, Public);
            var namespaceNode = new ApiNodeBuilder(Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(Assembly).WithMembers(namespaceNode).Build();
            var filtered = GetFiltered(new PrinterConfig(namespaceRegex, namespaceRegex), assemblyNode);

            Assert.That(filtered.Members, Is.Empty);
        }
        private static IApiNode GetFiltered(PrinterConfig printerConfig, IApiNode assemblyNode)
        {
            return FilteredApiNode.For(printerConfig, new[] { assemblyNode }).Single();
        }

    }
}
