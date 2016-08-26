using System.Collections.Generic;
using System.Linq;
using Gtc.AssemblyApi.ModelBuilder;

namespace Gtc.AssemblyApi.Comparison
{
    internal sealed class DifferentOnlyApiNodeComparison : IApiNodeComparison
    {
        private readonly IApiNodeComparison m_ApiNodeComparison;

        public DifferentOnlyApiNodeComparison(IApiNodeComparison apiNodeComparison)
        {
            m_ApiNodeComparison = apiNodeComparison;
        }

        public IReadOnlyCollection<IApiNodeComparison> MemberComparison
        {
            get { return m_ApiNodeComparison.MemberComparison.Where(n => n.IsDifferent).Select(n => new DifferentOnlyApiNodeComparison(n)).ToArray(); }
        }

        public IApiNode OldApiNode
        {
            get { return m_ApiNodeComparison.OldApiNode; }
        }

        public IApiNode NewApiNode
        {
            get { return m_ApiNodeComparison.NewApiNode; }
        }

        public SignatureDifferenceType SignatureDifferenceType
        {
            get { return m_ApiNodeComparison.SignatureDifferenceType; }
        }

        public bool IsDifferent
        {
            get { return m_ApiNodeComparison.IsDifferent; }
        }

        public string Kind
        {
            get { return m_ApiNodeComparison.Kind; }
        }

        public string Name
        {
            get { return m_ApiNodeComparison.Name; }
        }

        public string Namespace
        {
            get { return m_ApiNodeComparison.Namespace; }
        }

        public string Signature
        {
            get { return m_ApiNodeComparison.Signature; }
        }

        public string SymbolAccessibility
        {
            get { return m_ApiNodeComparison.SymbolAccessibility; }
        }
    }
}