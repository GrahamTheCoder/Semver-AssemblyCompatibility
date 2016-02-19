using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyApi.ModelBuilder;
using Microsoft.CodeAnalysis;

namespace AssemblyApiTests.Builders
{
    internal class ApiNodeBuilder
    {
        private readonly Accessibility m_SymbolAccessibility;
        private readonly SymbolKind m_SymbolKind;
        private readonly List<ApiNodeBuilder> m_Members = new List<ApiNodeBuilder>();
        private readonly string m_Name;
        private readonly string m_Signature;

        public ApiNodeBuilder(SymbolKind symbolKind = SymbolKind.NamedType, Accessibility accessibility = Accessibility.Public, string signature = null)
        {
            m_SymbolAccessibility = accessibility;
            m_SymbolKind = symbolKind;
            m_Signature = m_Name = signature ?? Path.GetRandomFileName().Substring(0, 8);
        }

        public ApiNodeBuilder WithMembers(params ApiNodeBuilder[] apiNodeBuilders)
        {
            m_Members.AddRange(apiNodeBuilders);
            return this;
        }

        public ApiNode Build()
        {
            var topLevel = new ApiNode(m_Signature, "", m_SymbolAccessibility, m_SymbolKind, m_Name);
            AddMembersAsChildrenOf(topLevel);
            return topLevel;
        }

        private ApiNode AddAsChildOf(ApiNode parent)
        {
            var newMember = parent.AddMember(m_Signature, (parent.Namespace + "." + m_Name).TrimStart('.'), m_SymbolAccessibility, m_SymbolKind, m_Name);
            AddMembersAsChildrenOf(newMember);
            return newMember;
        }

        private void AddMembersAsChildrenOf(ApiNode newMember)
        {
            foreach (var member in m_Members)
            {
                member.AddAsChildOf(newMember);
            }
        }
    }
}