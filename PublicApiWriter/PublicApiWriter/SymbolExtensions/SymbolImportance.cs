using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PublicApiWriter.SymbolExtensions
{
    internal static class SymbolImportance
    {
        /// <summary>
        /// A higher value indicates a more important member relative to its siblings
        /// </summary>
        public static int GetImportance(this ISymbol symbol)
        {
            int typeImportance = GetTypeImportance(symbol);
            int methodLikeMemberImportance = GetMethodLikeMemberImportance(symbol);
            int fieldImportance = GetFieldImportance(symbol);

            return typeImportance + methodLikeMemberImportance + fieldImportance;
        }

        private static int GetTypeImportance(ISymbol symbol)
        {
            var type = symbol as ITypeSymbol;
            var typeKindsByIncreasingImportance = new List<TypeKind?>
            {
                TypeKind.Class,
                TypeKind.Struct,
                TypeKind.Enum,
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

        private static int GetFieldImportance(ISymbol symbol)
        {
            var field = symbol as IFieldSymbol;
            var fieldKindsByImportance = new List<bool?>
            {
                field?.IsReadOnly,
                field?.IsStatic,
            };
            return fieldKindsByImportance.IndexOf(true);
        }
    }
}
