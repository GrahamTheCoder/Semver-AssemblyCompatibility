using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtc.AssemblyApi.Model;

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

        public string Name
        {
            get { return m_ApiNodeComparison.Name; }
        }

        public string Signature
        {
            get { return m_ApiNodeComparison.Signature; }
        }

        public override string ToString()
        {
            return m_ApiNodeComparison.DescribeChanges();
        }
    }
}