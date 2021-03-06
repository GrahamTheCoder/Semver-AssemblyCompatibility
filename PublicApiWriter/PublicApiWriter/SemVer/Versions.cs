﻿using System;
using System.Diagnostics;

namespace Gtc.AssemblyApi.SemVer
{
    [DebuggerDisplay("{AssemblyFileVersion.ToString()}")]
    public class Versions
    {
        public BinaryApiCompatibility Compatibility { get; }

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

        public Version AssemblyFileVersion { get; }
        public Version AssemblyInformationalVersion { get; }
        public Version AssemblyVersion { get; }
        public Version ExclusiveMaximumFileVersion { get; }
    }
}