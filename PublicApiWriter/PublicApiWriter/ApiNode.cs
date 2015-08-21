using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace PublicApiWriter
{
    internal sealed class ApiNode
    {
        private readonly Accessibility m_SymbolAccessibility;
        private readonly HashSet<ApiNode> m_Members = new HashSet<ApiNode>();

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility)
        {
            m_SymbolAccessibility = symbolAccessibility;
            Qualifier = @namespace;
            Signature = signature;
        }

        public string Qualifier { get; }
        public string Signature{ get; }
        public void AddMember(ApiNode member)
        {
            m_Members.Add(member);
        }
        public bool Contains(ApiNode member)
        {
            return member.Contains(member);
        }
        public async Task Write(TextWriter file, Accessibility accessibility, CancellationToken cancellationToken, bool recurse = true)
        {
            if (m_SymbolAccessibility >= accessibility)
            {
                file.WriteLine(Signature);

                if (recurse)
                {
                    var indentedTextWriter = new IndentedTextWriter(file, " ") { Indent = 2 };
                    foreach (var member in m_Members)
                    {
                        await member.Write(indentedTextWriter, accessibility, cancellationToken);
                    }
                }
            }
        }

        #region Equality members for Signature

        private bool Equals(ApiNode other)
        {
            return string.Equals(Signature, other.Signature);
        }

        /// <summary>
        /// Returns true iff <paramref name="obj"/> is an <see cref="ApiNode"/> with the same <see cref="Signature"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApiNode) obj);
        }

        public override int GetHashCode()
        {
            return Signature.GetHashCode();
        }

        public static bool operator ==(ApiNode left, ApiNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ApiNode left, ApiNode right)
        {
            return !Equals(left, right);
        }
        #endregion

    }
}