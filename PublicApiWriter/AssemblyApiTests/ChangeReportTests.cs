using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.SemVer;
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
            var differenceString = await GetDifferencesString(comparison);

            Assert.That(differenceString, Does.Not.Contain("same"));
        }

        private static async Task<string> GetDifferencesString(IReadOnlyCollection<ApiNodeComparison> comparison)
        {
            using (var sw = new StringWriter())
            {
                await ApiComparisonWriter.Write(comparison.GetDifferences(), sw);
                return sw.ToString();
            }
        }
    }
}
