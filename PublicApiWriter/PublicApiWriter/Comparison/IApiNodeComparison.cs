﻿using System.Collections.Generic;
using AssemblyApi.ModelBuilder;

namespace AssemblyApi.Comparison
{
    internal interface IApiNodeComparison
    {
        IApiNode OldApiNode { get; }
        IApiNode NewApiNode { get; }
        IReadOnlyCollection<IApiNodeComparison> MemberComparison { get; }
        SignatureDifferenceType SignatureDifferenceType { get; }
        bool IsDifferent { get; }
        string Kind { get; }
        string Name { get; }
        string Namespace { get; }
        string Signature { get; }
        string SymbolAccessibility { get; }
        string ToString();
    }
}