using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.SymbolDisplayMemberOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayKindOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayGenericsOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayParameterOptions;

namespace AssemblyApi.SymbolExtensions
{
    internal static class SymbolFormatter
    {
        private static readonly SymbolDisplayFormat s_Format = CreateSignatureFormat();
        private static readonly SymbolDisplayPart[] s_CommaSpace = { new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, ","), new SymbolDisplayPart(SymbolDisplayPartKind.Space, null, " ") };
        private static readonly SymbolDisplayPart[] s_InheritsFrom = { new SymbolDisplayPart(SymbolDisplayPartKind.Text, null, " : ") };
        private static readonly TypeKind?[] s_TypeKindsWithUserSpecifiedSuperTypes = { TypeKind.Class, TypeKind.Interface, TypeKind.Struct};
        public static string GetSignature(this ISymbol symbol)
        {
            var defaultParts = SymbolDisplay.ToDisplayParts(symbol, s_Format);
            var allParts = WithSupertypes(defaultParts, symbol as INamedTypeSymbol);
            return allParts.ToDisplayString().Replace($"{symbol.ContainingNamespace}.", "");
        }

        private static ImmutableArray<SymbolDisplayPart> WithSupertypes(ImmutableArray<SymbolDisplayPart> defaultParts, INamedTypeSymbol type)
        {
            if (type == null || !s_TypeKindsWithUserSpecifiedSuperTypes.Contains(type.TypeKind)) return defaultParts;
            var baseTypes = new[] { type.BaseType }.Where(NonImpliedBaseType);
            var inheritsFrom = baseTypes.Concat(GetInterfaces(type)).Select(GetSimpleTypeName).ToList();
            var inheritanceSuffix = inheritsFrom.Any() ? s_InheritsFrom.Concat(CommaSeparate(inheritsFrom)) : new SymbolDisplayPart[0];
            return WithInheritsFrom(defaultParts, inheritanceSuffix);
        }

        private static bool NonImpliedBaseType(INamedTypeSymbol t)
        {
            return t != null && t.SpecialType != SpecialType.System_Object && t.SpecialType != SpecialType.System_Enum;
        }

        private static ImmutableArray<SymbolDisplayPart> WithInheritsFrom(ImmutableArray<SymbolDisplayPart> defaultParts, IEnumerable<SymbolDisplayPart> inheritanceSuffix)
        {
            var spaceBeforeFirstTypeConstraint = defaultParts.IndexOf(new SymbolDisplayPart(SymbolDisplayPartKind.Keyword, null, "where")) - 1;
            var locationToInsert = spaceBeforeFirstTypeConstraint >= 0 ? spaceBeforeFirstTypeConstraint : defaultParts.Count();
            return defaultParts.InsertRange(locationToInsert, inheritanceSuffix);
        }

        private static IEnumerable<SymbolDisplayPart> CommaSeparate(List<ImmutableArray<SymbolDisplayPart>> inheritsFrom)
        {
            var lastIndex = inheritsFrom.Count() - 1;
            return inheritsFrom.SelectMany((parts, i) => MaybeAddCommaSpace(parts, i, lastIndex));
        }

        private static IEnumerable<SymbolDisplayPart> MaybeAddCommaSpace(ImmutableArray<SymbolDisplayPart> parts, int currentIndex, int lastIndex)
        {
            return currentIndex == lastIndex ? parts : parts.Concat(s_CommaSpace);
        }

        private static ImmutableArray<INamedTypeSymbol> GetInterfaces(INamedTypeSymbol type)
        {
            // Don't need the recursion for correctness, but it'll be easier to see an API change's full reach this way
            // Also the obvious alternative (Interfaces) looked like it might include interfaces used to constrain type parameters and this is the wrong place for them
            return type.AllInterfaces;
        }

        private static ImmutableArray<SymbolDisplayPart> GetSimpleTypeName(INamedTypeSymbol t)
        {
            return SymbolDisplay.ToDisplayParts(t, SymbolDisplayFormat.CSharpErrorMessageFormat);
        }

        private static SymbolDisplayFormat CreateSignatureFormat()
        {
            var defaultFormat = SymbolDisplayFormat.CSharpErrorMessageFormat;
            return defaultFormat
                .WithMemberOptions(IncludeExplicitInterface | IncludeParameters | IncludeModifiers | SymbolDisplayMemberOptions.IncludeType)
                .WithKindOptions(IncludeMemberKeyword | IncludeNamespaceKeyword | IncludeTypeKeyword)
                .WithGenericsOptions(IncludeTypeConstraints | IncludeTypeParameters | IncludeVariance)
                .WithParameterOptions(IncludeExtensionThis | IncludeOptionalBrackets | IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType);
        }

    }
}