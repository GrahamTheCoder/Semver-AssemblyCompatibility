using Gtc.AssemblyApi.ModelBuilder;
using Gtc.AssemblyApiTests.Builders;
using Microsoft.CodeAnalysis;

namespace Gtc.AssemblyApiTests
{
    internal static class ApiBuilder
    {
        public static ApiNode CreateApi(string uniquebit)
        {
            var type = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public, $"Type{uniquebit}");
            var nonUnique = new ApiNodeBuilder(SymbolKind.NamedType, Accessibility.Public, "NonUniqueType");
            var namespaceNode = new ApiNodeBuilder(SymbolKind.Namespace, Accessibility.Public, $"Namespace{uniquebit}").WithMembers(type, nonUnique);
            var assemblyNode =
                new ApiNodeBuilder(SymbolKind.Assembly, Accessibility.Public, $"Assembly{uniquebit.GetHashCode()}").WithMembers(namespaceNode).Build();
            return assemblyNode;
        }
    }
}