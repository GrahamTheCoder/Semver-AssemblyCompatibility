using System;
using JetBrains.Annotations;

namespace Gtc.AssemblyApi.CodeAnalysis
{
    internal partial class ApiReader
    {
        [UsedImplicitly] //MSBuild for consuming projects won't copy the C# dll otherwise and hence won't be able to load csprojs https://stackoverflow.com/a/38668082/1128762
        private static Type[] s_ForceAssemblyReferenceRequiredAtRuntime =
        {
            typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions),
            typeof(Microsoft.CodeAnalysis.Host.Mef.DesktopMefHostServices),
            typeof(Microsoft.Build.Execution.BuildManager),
            typeof(Microsoft.Build.Tasks.MSBuild)
        };
    }
}
