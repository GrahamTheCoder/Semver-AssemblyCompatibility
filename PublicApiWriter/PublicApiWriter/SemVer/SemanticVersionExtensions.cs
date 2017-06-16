using System;

namespace Gtc.AssemblyApi.SemVer
{
    public static class SemanticVersionExtensions
    {
        public static Version GetNewSemanticVersion(this Version oldSemVer, BinaryApiCompatibility compatibility)
        {
            switch (compatibility)
            {
                case BinaryApiCompatibility.Identical:
                    return WithBuildIncremented(oldSemVer);
                case BinaryApiCompatibility.BackwardsCompatible:
                    return WithMinorIncremented(oldSemVer);
                case BinaryApiCompatibility.Incompatible:
                    return new Version(oldSemVer.Major + 1, 0, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(compatibility));
            }
        }

        public static Version WithMinorIncremented(this Version oldSemVer)
        {
            return new Version(oldSemVer.Major, oldSemVer.Minor + 1, 0, 0);
        }

        public static Version WithBuildIncremented(this Version oldSemVer)
        {
            return new Version(oldSemVer.Major, oldSemVer.Minor, oldSemVer.Build + 1, 0);
        }

        public static Version WithAllPartsSet(this Version version)
        {
            return version == null
                ? null
                : new Version(version.Major, version.Minor, version.Build, version.Revision != -1 ? version.Revision : 0);
        }
    }
}