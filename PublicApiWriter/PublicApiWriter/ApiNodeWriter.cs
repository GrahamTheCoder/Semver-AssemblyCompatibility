using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PublicApiWriter
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
                await file.WriteAsync(apiNode.Signature);
                if (recurse)
                {
                    var orderedMembers = apiNode.Members.OrderByDescending(m => m.Importance).ThenBy(m => m.Name);
                    await WriteMembers(apiNode, file, cancellationToken, orderedMembers);
                }
            }
        }

        private async Task WriteMembers(ApiNode apiNode, TextWriter file, CancellationToken cancellationToken, IOrderedEnumerable<ApiNode> orderedMembers)
        {
            if (apiNode.Kind == Microsoft.CodeAnalysis.SymbolKind.Property || apiNode.Kind == Microsoft.CodeAnalysis.SymbolKind.Event)
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

        private static string GetShortNameFromAutoGeneratedMember(ApiNode m)
        {
            var indexOfUnderscore = m.Name.IndexOf("_");
            var nameBeforeUnderscore = indexOfUnderscore > 2
                ? m.Name.Substring(0, indexOfUnderscore)
                : m.Name; //If someone creates a custom getter/setter/adder/remover/raiser using IL, make a vague effort to notice and not blow up
            return $"{m.SymbolAccessibility.ToString().ToLowerInvariant()} {nameBeforeUnderscore}";
        }
    }
}