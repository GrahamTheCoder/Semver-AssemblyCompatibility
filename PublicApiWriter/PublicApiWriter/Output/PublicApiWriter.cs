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
                foreach (var node in nodes.OrderBy(n => n.Name))
                {
                    await m_ApiNodeWriter.Write(node, file, cancellationToken);
                }
            }
        }
    }
}