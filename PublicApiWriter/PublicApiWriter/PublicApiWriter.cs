using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace PublicApiWriter
{
    internal class PublicApiWriter
    {
        private readonly ApiNodeWriter m_ApiNodeWriter;

        public PublicApiWriter(PrinterConfig apiNodeWriter)
        {
            m_ApiNodeWriter = new ApiNodeWriter(apiNodeWriter);
        }

        public async Task Write(IEnumerable<ApiNode> assemblies, string outputFile, CancellationToken cancellationToken)
        {
            using (var file = new StreamWriter(outputFile, false))
            {
                foreach (var assembly in assemblies)
                {
                    await m_ApiNodeWriter.Write(assembly, file, cancellationToken);
                }
            }
        }
    }
}