using System;
using System.Diagnostics;

namespace AssemblyApiTests
{
    [DebuggerDisplay("{AssemblyFileVersion.ToString()}")]
    internal class Versions
    {
        public BinaryApiCompatibility Compatibility { get; private set; }

        public Versions(Version oldFileVersion, BinaryApiCompatibility compatibility)
        {
            Compatibility = compatibility;
            var oldSemanticVersion = new Version(oldFileVersion.Major, oldFileVersion.Minor, oldFileVersion.Build, 0);
            var newSemanticVersion = oldSemanticVersion.GetNewSemanticVersion(compatibility);
            AssemblyFileVersion = newSemanticVersion;
            AssemblyInformationalVersion = AssemblyFileVersion;
            AssemblyVersion = new Version(newSemanticVersion.Major, 0, 0, 0);
            ExclusiveMaximumFileVersion = AssemblyFileVersion.WithMinorIncremented();
        }

        public Version AssemblyFileVersion { get; private set; }
        public Version AssemblyInformationalVersion { get; private set; }
        public Version AssemblyVersion { get; private set; }
        public Version ExclusiveMaximumFileVersion { get; private set; }
    }
}