using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.SymbolDisplayMemberOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayKindOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayGenericsOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayParameterOptions;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace PublicApiWriter.SymbolExtensions
{
    internal static class SymbolFormatter
    {
        private static SymbolDisplayFormat s_Format = CreateSignatureFormat();
        private static SymbolDisplayPart[] s_CommaSpace = new[] { new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, ","), new SymbolDisplayPart(SymbolDisplayPartKind.Space, null, " ") };
        private static SymbolDisplayPart[] s_InheritsFrom = new[] { new SymbolDisplayPart(SymbolDisplayPartKind.Text, null, " : ") };

        public static string GetSignature(this ISymbol symbol)
        {
            var defaultParts = SymbolDisplay.ToDisplayParts(symbol, s_Format);
            var allParts = WithTypeSpecializations(defaultParts, symbol as INamedTypeSymbol);
            return allParts.ToDisplayString();
        }

        private static ImmutableArray<SymbolDisplayPart> WithTypeSpecializations(ImmutableArray<SymbolDisplayPart> defaultParts, INamedTypeSymbol type)
        {
            if (type == null) return defaultParts;
            var baseTypes = new[] { type.BaseType }.Where(t => t != null && t.SpecialType != SpecialType.System_Object);
            var inheritsFrom = baseTypes.Concat(GetInterfaces(type)).Select(t => GetSimpleTypeName(t)).ToList();
            var inheritanceSuffix = inheritsFrom.Any() ? s_InheritsFrom.Concat(CommaSeparate(inheritsFrom)) : new SymbolDisplayPart[0];
            return WithInheritsFrom(defaultParts, inheritanceSuffix);
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