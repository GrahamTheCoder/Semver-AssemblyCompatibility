using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.IO;

namespace Gtc.AssemblyApi.Model
{
    internal class FilteredApiNode : IApiNode
    {
        private readonly IApiNode m_ApiNodeImplementation;
        private readonly PrinterConfig m_PrinterConfig;

        public FilteredApiNode(IApiNode apiNodeImplementation, PrinterConfig printerConfig)
        {
            m_ApiNodeImplementation = apiNodeImplementation;
            m_PrinterConfig = printerConfig;
        }

        public IEnumerable<IApiNode> Members
        {
            get { return m_ApiNodeImplementation.Members.Where(IsVisibleApiMember).Select(n => new FilteredApiNode(n, m_PrinterConfig)); }
        }

        private bool IsVisibleApiMember(IApiNode apiNode)
        {
            if (apiNode.Kind == SymbolKind.Namespace || apiNode.Kind == SymbolKind.Assembly)
            {
                return apiNode.Members.Any(IsVisibleApiMember);
            }
            else
            {
                return m_PrinterConfig.ShouldPrint(apiNode.Namespace, apiNode.SymbolAccessibility);
            }
        }

        public long Importance => m_ApiNodeImplementation.Importance;

        public SymbolKind Kind => m_ApiNodeImplementation.Kind;

        public string Name => m_ApiNodeImplementation.Name;

        public string Namespace => m_ApiNodeImplementation.Namespace;

        public string Signature => m_ApiNodeImplementation.Signature;

        public Accessibility SymbolAccessibility => m_ApiNodeImplementation.SymbolAccessibility;

        public Dictionary<string, List<string>> Attributes => m_ApiNodeImplementation.Attributes;

        public static IEnumerable<IApiNode> For(PrinterConfig printerConfig, IEnumerable<IApiNode> solutionNodes)
        {
            return solutionNodes.Select(n => new FilteredApiNode(n, printerConfig));
        }
    }
}