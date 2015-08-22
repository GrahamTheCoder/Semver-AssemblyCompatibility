using System.CodeDom.Compiler;
using System.IO;
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
                await file.WriteLineAsync(apiNode.Signature);

                if (recurse)
                {
                    var indentedTextWriter = new IndentedTextWriter(file, " ") { Indent = 2 };
                    foreach (var member in apiNode.Members)
                    {
                        await Write(member, indentedTextWriter, cancellationToken);
                    }
                }
            }
        }
    }
}