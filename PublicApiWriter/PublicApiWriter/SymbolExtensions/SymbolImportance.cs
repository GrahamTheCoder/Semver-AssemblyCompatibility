using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace PublicApiWriter.SymbolExtensions
{
    internal static class SymbolImportance
    {
        /// <summary>
        /// A higher value indicates a more important member relative to its siblings
        /// </summary>
        public static long GetImportance(this ISymbol symbol)
        {
            long typeImportance = GetTypeImportance(symbol);
            long methodLikeMemberImportance = GetMethodLikeMemberImportance(symbol);
            long fieldImportance = GetFieldImportance(symbol);
            return typeImportance + methodLikeMemberImportance + fieldImportance;
        }

        private static long GetTypeImportance(ISymbol symbol)
        {
            var type = symbol as ITypeSymbol;
            var typeKindsByIncreasingImportance = new List<TypeKind?>
            {
                TypeKind.Class,
                TypeKind.Struct,
                TypeKind.Enum,
                TypeKind.Interface,
            };
            return 1 - typeKindsByIncreasingImportance.IndexOf(type?.TypeKind);
        }

        private static long GetMethodLikeMemberImportance(ISymbol symbol)
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
            return 1 - methodPropertiesByIncreasingImportance.IndexOf(true);
        }

        private static long GetFieldImportance(ISymbol symbol)
        {
            var field = symbol as IFieldSymbol;
            var fieldKindsByImportance = new List<bool?>
            {
                field?.IsReadOnly,
                field?.IsStatic,
            };
            return field?.ContainingType?.TypeKind != TypeKind.Enum
                ? 1 - fieldKindsByImportance.IndexOf(true)
                : Convert.ToInt64(field.ConstantValue);
        }
    }
}
