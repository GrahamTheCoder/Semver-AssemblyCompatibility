using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.SemVer;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    class ChangeReportTests
    {
        [Test]
        public void GivenIncompatibleApiThenMajorVersionIncreases()
        {
            var oldApi = ApiBuilder.CreateApi("1");
            var newApi = ApiBuilder.CreateApi("2");
            var sameApi = ApiBuilder.CreateApi("same");
            var comparison = ApiNodeComparison.Compare(new [] { sameApi, oldApi}, new [] { sameApi, newApi});
            var differentNodesPerAssembly = comparison.GetDifferences();
            foreach (var apiNodeComparison in differentNodesPerAssembly)
            {
                Assert.That(apiNodeComparison.ToString(), Does.Not.Contain("same"));
            }
        }
    }
}
