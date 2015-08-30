using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicApiWriter
{
    internal static class SymbolExtensions
    {
        /// <summary>
        /// A higher value indicates a more important member relative to its siblings
        /// </summary>
        public static int GetImportance(this ISymbol symbol)
        {
            int typeImportance = GetTypeImportance(symbol);
            int methodLikeMemberImportance = GetMethodLikeMemberImportance(symbol);
            return typeImportance + methodLikeMemberImportance;
        }

        private static int GetTypeImportance(ISymbol symbol)
        {
            var type = symbol as ITypeSymbol;
            var typeKindsByIncreasingImportance = new List<TypeKind?>
            {
                TypeKind.Class,
                TypeKind.Enum,
                TypeKind.Struct,
                TypeKind.Interface,
            };
            return typeKindsByIncreasingImportance.IndexOf(type?.TypeKind);
        }

        private static int GetMethodLikeMemberImportance(ISymbol symbol)
        {
            var method = symbol as IMethodSymbol;
            var methodPropertiesByIncreasingImportance = new List<bool>
            {
                symbol is IEventSymbol,
                symbol is IPropertySymbol,
                method?.MethodKind == MethodKind.Destructor,
                method?.MethodKind == MethodKind.Constructor,
                method?.MethodKind == MethodKind.StaticConstructor,
            };
            var methodImportance = methodPropertiesByIncreasingImportance.IndexOf(true);
            return methodImportance;
        }
    }
}
