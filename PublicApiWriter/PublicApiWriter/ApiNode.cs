using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace PublicApiWriter
{
    internal sealed class ApiNode
    {
        private readonly HashSet<ApiNode> m_Members = new HashSet<ApiNode>();

        public ApiNode(string signature, string @namespace, Accessibility symbolAccessibility)
        {
            SymbolAccessibility = symbolAccessibility;
            Namespace = @namespace;
            Signature = signature;
        }

        public string Signature { get; }
        public string Namespace { get; }
        public Accessibility SymbolAccessibility { get; }
        public IEnumerable<ApiNode> Members => m_Members;

        public void AddMember(ApiNode member)
        {
            m_Members.Add(member);
        }
        public bool Contains(ApiNode member)
        {
            return member.Contains(member);
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