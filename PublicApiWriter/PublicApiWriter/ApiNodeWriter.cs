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
                    var indentedTextWriter = new IndentedTextWriter(file, " ") { Indent = 2 };
                    var orderedMembers = apiNode.Members.OrderByDescending(m => m.Importance).ThenBy(m => m.Name);
                    foreach (var member in orderedMembers)
                    {
                        await Write(member, indentedTextWriter, cancellationToken);
                    }
                }
            }
        }
    }
}