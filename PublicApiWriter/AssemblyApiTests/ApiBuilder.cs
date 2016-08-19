using AssemblyApi.ModelBuilder;
using AssemblyApiTests.Builders;
using Microsoft.CodeAnalysis;

namespace AssemblyApiTests
{
    internal static class ApiBuilder
    {
        public static ApiNode CreateApi(string uniquebit)
        {
            var type = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public, $"Type{uniquebit}");
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, Accessibility.Public, $"Namespace{uniquebit}").WithMembers(type);
            var assemblyNode =
                new ApiNodeBuilder(SymbolKind.Assembly, Accessibility.Public, $"Assembly{uniquebit}").WithMembers(namespaceNode).Build();
            return assemblyNode;
        }
    }
}