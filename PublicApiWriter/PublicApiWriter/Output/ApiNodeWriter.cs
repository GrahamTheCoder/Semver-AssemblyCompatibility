using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace AssemblyApi.Output
{
    internal class ApiNodeWriter
    {
        private readonly PrinterConfig m_PrinterConfig;

        public ApiNodeWriter(PrinterConfig printerConfig)
        {
            m_PrinterConfig = printerConfig;
        }

        public async Task Write(ApiNode apiNode, TextWriter file, CancellationToken cancellationToken, bool recurse = true)
        {
            if (m_PrinterConfig.ShouldPrint(apiNode.Namespace, apiNode.SymbolAccessibility))
            {
                file.WriteLine(); //Indentation is only correct after a newline (note: WriteLineAsync doesn't seem to do indentation at all)
                await file.WriteAsync($"{AccessibilityPrefix(apiNode)}{apiNode.Signature}");
                if (recurse)
                {
                    var orderedMembers = MembersInCanonicalOrder(apiNode);
                    await WriteMembers(apiNode, file, cancellationToken, orderedMembers);
                }
            }
        }

        private static IOrderedEnumerable<ApiNode> MembersInCanonicalOrder(ApiNode apiNode)
        {
            return apiNode.Members.OrderBy(m => m.Importance).ThenBy(m => m.Name);
        }

        private async Task WriteMembers(ApiNode apiNode, TextWriter file, CancellationToken cancellationToken, IOrderedEnumerable<ApiNode> orderedMembers)
        {
            if (apiNode.Kind == Property || apiNode.Kind == Event)
            {
                await WriteAutogeneratedShortnamesInline(file, cancellationToken, orderedMembers);
            }
            else
            {
                await WriteMembersIndented(file, cancellationToken, orderedMembers);
            }
        }

        private async Task WriteMembersIndented(TextWriter file, CancellationToken cancellationToken, IOrderedEnumerable<ApiNode> orderedMembers)
        {
            var indentedTextWriter = new IndentedTextWriter(file, " ") { Indent = 2 };
            foreach (var member in orderedMembers)
            {
                await Write(member, indentedTextWriter, cancellationToken);
            }
        }

        private async Task WriteAutogeneratedShortnamesInline(TextWriter file, CancellationToken cancellationToken, IOrderedEnumerable<ApiNode> orderedMembers)
        {
            var accessibleMembers = orderedMembers.Where(m => m_PrinterConfig.ShouldPrint(m.Namespace, m.SymbolAccessibility)).Select(m => GetShortNameFromAutoGeneratedMember(m));
            var memberString = $" {{ {string.Join("; ", accessibleMembers)}; }}";
            await file.WriteAsync(memberString);
        }

        private static string GetShortNameFromAutoGeneratedMember(ApiNode autoGeneratedNode)
        {
            var indexOfUnderscore = autoGeneratedNode.Name.IndexOf("_");
            if (indexOfUnderscore < 3) { throw new ArgumentOutOfRangeException("Expected a node with a name starting with something like get_, set_ ,add_ or remove_", nameof(autoGeneratedNode)); }

            var nameBeforeUnderscore = autoGeneratedNode.Name.Substring(0, indexOfUnderscore);
            return $"{AccessibilityPrefix(autoGeneratedNode)}{nameBeforeUnderscore}";
        }

        private static string AccessibilityPrefix(ApiNode node)
        {
            return node.Kind == Namespace || node.Kind == Assembly || node.SymbolAccessibility == Accessibility.NotApplicable ? ""
                : $"{node.SymbolAccessibility.ToString().ToLowerInvariant()} ";
        }
    }
}