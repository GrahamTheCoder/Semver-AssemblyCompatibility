using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AssemblyApi.Output
{
    internal class PublicApiWriter
    {
        private readonly ApiNodeWriter m_ApiNodeWriter;

        public PublicApiWriter(PrinterConfig apiNodeWriter)
        {
            m_ApiNodeWriter = new ApiNodeWriter(apiNodeWriter);
        }

        public async Task Write(IEnumerable<ApiNode> nodes, string outputFile, CancellationToken cancellationToken)
        {
            using (var file = new StreamWriter(outputFile, false))
            {
                file.WriteLine("This autogenerated file contains the binary-compatibility API. It can be checked into your VCS to help track changes.");
                file.WriteLine("- Edits and removals from this file are caused by binary-incompatible changes");
                file.WriteLine("- Additions to this file are binary-compatible in most cases.");
                file.WriteLine("For more information, see: https://github.com/GrahamTheCoder/Semver-AssemblyCompatibility");

                foreach (var node in nodes.OrderBy(n => n.Name))
                {
                    await m_ApiNodeWriter.Write(node, file, cancellationToken);
                }
            }
        }
    }
}