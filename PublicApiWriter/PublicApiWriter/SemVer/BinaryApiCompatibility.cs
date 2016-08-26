namespace Gtc.AssemblyApi.SemVer
{
    public enum BinaryApiCompatibility
    {
        /// <summary>
        /// Public members have been removed - minimum compatibility
        /// </summary>
        Incompatible,

        /// <summary>
        /// No public members have been removed, but some have been added
        /// Does not guard against edge cases such as these: http://blogs.msdn.com/b/ericlippert/archive/2012/01/09/every-public-change-is-a-breaking-change.aspx
        /// </summary>
        BackwardsCompatible,

        /// <summary>
        /// The public interface has not changed - maximum compatibility
        /// </summary>
        Identical
    }
}