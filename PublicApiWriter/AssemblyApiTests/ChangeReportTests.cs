using System.Threading.Tasks;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.Extensions;
using Gtc.AssemblyApiTests.Builders;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    class ChangeReportTests
    {
        [Test]
        public async Task GivenIncompatibleApiThenMajorVersionIncreases()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var sameApi = ApiBuilder.CreateApi("same");
            var comparison = ApiNodeComparison.Compare(new [] { sameApi, oldApi}, new [] { sameApi, newApi});
            var differenceString = await comparison.GetDifferencesString();

            Assert.That(differenceString, Does.Not.Contain("same"));
        }
    }
}
