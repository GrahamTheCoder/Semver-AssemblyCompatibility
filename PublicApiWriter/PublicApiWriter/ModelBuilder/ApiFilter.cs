using System.Collections.Generic;
using System.Linq;
using AssemblyApi.Output;
using Microsoft.CodeAnalysis;

namespace AssemblyApi.ModelBuilder
{
    internal class ApiFilter
    {
        private readonly PrinterConfig m_PrinterConfig;

        public ApiFilter(PrinterConfig printerConfig)
        {
            m_PrinterConfig = printerConfig;
        }

        public void ApplyTo(IEnumerable<ApiNode> apiNodes)
        {
            RemoveFilteredMembers(apiNodes);
        }

        private void RemoveFilteredMembers(IEnumerable<ApiNode> apiNodes)
        {
            foreach (var apiNode in apiNodes)
            {
                ApplyTo(apiNode.Members);
                apiNode.RemoveDescendantsWhere(IsNotVisibleApiMember);
            }
        }

        private bool IsNotVisibleApiMember(ApiNode apiNode)
        {
            if (apiNode.Kind == SymbolKind.Namespace || apiNode.Kind == SymbolKind.Assembly)
            {
                return !apiNode.Members.Any();
            }
            else
            {
                var shouldPrint = m_PrinterConfig.ShouldPrint(apiNode.Namespace, apiNode.SymbolAccessibility);
                return !shouldPrint;
            }
        }
    }
}