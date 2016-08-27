using System.Linq;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.Model;
using Gtc.AssemblyApiTests.Builders;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    [TestFixture]
    public class ApiFilterTests
    {
        [TestCase(Accessibility.Public, @"SomeOtherNamespace", 1)]
        [TestCase(Accessibility.Private, @"SomeOtherNamespace", 0)]
        [TestCase(Accessibility.Public, @"Api\.Tests", 0)]
        public void NamespaceOnlyExcludedWhenContentsAreExcluded(Accessibility typeAccessibility, string namespaceExclusionRegex, int expectedMembers)
        {
            var type = new ApiNodeBuilder(SymbolKind.NamedType, typeAccessibility);
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(SymbolKind.Assembly).WithMembers(namespaceNode).Build();
            var filtered = GetFiltered(new PrinterConfig("", namespaceExclusionRegex), assemblyNode);

            Assert.That(filtered.Members.Count<IApiNode>(), Is.EqualTo(expectedMembers));
        }

        [Test]
        public void IncludedIfMatchedByInclude()
        {
            var toIncludeRegex = @"Api\.Tests";
            var type = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public);
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(SymbolKind.Assembly).WithMembers(namespaceNode).Build();
            var filtered = GetFiltered(new PrinterConfig(toIncludeRegex, ""), assemblyNode);

            Assert.That(filtered.Members, Is.Not.Empty);
        }

        [Test]
        public void AllExcludedIfDoNotMatchInclude()
        {
            var toInclude = "Api.NonExistent";
            var type = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public);
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(SymbolKind.Assembly).WithMembers(namespaceNode).Build();

            var filtered = GetFiltered(new PrinterConfig(toInclude, ""), assemblyNode);
            Assert.That(filtered.Members, Is.Empty);
        }

        [Test]
        public void ExcludeTakesPrecedenceOverInclude()
        {
            var namespaceRegex = @"Api\.Tests";
            var type = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public);
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, signature: "Api.Tests").WithMembers(type);
            var assemblyNode = new ApiNodeBuilder(SymbolKind.Assembly).WithMembers(namespaceNode).Build();
            var filtered = GetFiltered(new PrinterConfig(namespaceRegex, namespaceRegex), assemblyNode);

            Assert.That(filtered.Members, Is.Empty);
        }
        private static IApiNode GetFiltered(PrinterConfig printerConfig, IApiNode assemblyNode)
        {
            return FilteredApiNode.For(printerConfig, new[] { assemblyNode }).Single();
        }

    }
}
