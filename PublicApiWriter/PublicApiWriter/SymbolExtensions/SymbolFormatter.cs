using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.SymbolDisplayMemberOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayKindOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayGenericsOptions;
using static Microsoft.CodeAnalysis.SymbolDisplayParameterOptions;

namespace PublicApiWriter.SymbolExtensions
{
    internal static class SymbolFormatter
    {
        private static SymbolDisplayFormat s_Format = CreateSignatureFormat();

        public static string GetSignature(this ISymbol symbol)
        {
            return SymbolDisplay.ToDisplayString(symbol, s_Format);
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