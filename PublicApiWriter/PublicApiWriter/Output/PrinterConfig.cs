using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssemblyApi.Output
{
    internal class PrinterConfig
    {
        public string[] IncludeSignatureRegexes { get; }
        public string[] ExcludeSignatureRegexes { get; }
        public Accessibility Accessibility { get; } = Accessibility.Public;

        public PrinterConfig(string semiColonSeparatedIncludeRegexes, string semiColonSeparatedExcludeRegexes)
        {
            var splitters = new[] { ";" };
            IncludeSignatureRegexes = semiColonSeparatedIncludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            ExcludeSignatureRegexes = semiColonSeparatedExcludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool ShouldPrint(string @namespace, Accessibility symbolAccessibility)
        {
            return (symbolAccessibility >= Accessibility || symbolAccessibility == Accessibility.NotApplicable)
                   && IsIncluded(@namespace)
                   && !IsExcluded(@namespace);
        }

        private bool IsIncluded(string ns)
        {
            return !IncludeSignatureRegexes.Any() || IncludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
        }

        private bool IsExcluded(string ns)
        {
            return ExcludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
        }
    }
}
