using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace AssemblyApi.IO
{
    internal class PrinterConfig
    {
        private readonly Accessibility m_MinAccessibility;
        private readonly string[] m_IncludeSignatureRegexes;
        private readonly string[] m_ExcludeSignatureRegexes;

        public PrinterConfig(string semiColonSeparatedIncludeRegexes, string semiColonSeparatedExcludeRegexes, Accessibility minAccessibility = Accessibility.Public)
        {
            var splitters = new[] { ";" };
            m_IncludeSignatureRegexes = semiColonSeparatedIncludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            m_ExcludeSignatureRegexes = semiColonSeparatedExcludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            m_MinAccessibility = minAccessibility;
        }

        public bool ShouldPrint(string @namespace, Accessibility symbolAccessibility)
        {
            return (symbolAccessibility >= m_MinAccessibility || symbolAccessibility == Accessibility.NotApplicable)
                   && IsIncluded(@namespace)
                   && !IsExcluded(@namespace);
        }

        private bool IsIncluded(string ns)
        {
            return !m_IncludeSignatureRegexes.Any() || m_IncludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
        }

        private bool IsExcluded(string ns)
        {
            var isExcluded = m_ExcludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
            return isExcluded;
        }
    }
}
