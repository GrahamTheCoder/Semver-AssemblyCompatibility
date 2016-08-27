using System;
using System.Collections.Generic;
using Gtc.AssemblyApi.Model;

namespace Gtc.AssemblyApi.Comparison
{
    internal interface IApiNodeComparison
    {
        IApiNode OldApiNode { get; }
        IApiNode NewApiNode { get; }
        IReadOnlyCollection<IApiNodeComparison> MemberComparison { get; }
        SignatureDifferenceType SignatureDifferenceType { get; }
        bool IsDifferent { get; }
        string Name { get; }
        string Signature { get; }
        string ToString();
        T Get<T>(Func<IApiNode, T> describe);
    }
}