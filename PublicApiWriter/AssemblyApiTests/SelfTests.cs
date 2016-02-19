using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AssemblyApi;
using AssemblyApi.ModelBuilder;
using AssemblyApiTests.Utils;
using NUnit.Framework;

namespace AssemblyApiTests
{
    [TestFixture]
    public class SelfTests
    {
        private readonly FileInfo m_ThisProjectFile;

        public SelfTests()
        {
            var solutionDirectory = new DirectoryInfo(Environment.CurrentDirectory + @"\..\..\");
            m_ThisProjectFile = solutionDirectory.GetFiles("*.csproj").First();
        }

        [Test]
        public void AnalyzingSelfFindsThisMethodAndOtherStuff()
        {
            using (var tempFileManager = new TempFileManager())
            {
                var outputFile = tempFileManager.GetNew();
                RunMain(m_ThisProjectFile.FullName, outputFile.FullName, "TestCode;AssemblyApiTests", @"OurCode;AssemblyApi\.");
                var lines = File.ReadAllLines(outputFile.FullName);

                var linesContainingThisMethod = lines.Where(l => l.Contains(nameof(AnalyzingSelfFindsThisMethodAndOtherStuff))).ToList();
                Assert.That(linesContainingThisMethod, Has.Count.EqualTo(1));

                Assert.That(lines, Has.Length.GreaterThan(15));
            }
        }

        [Test]
        public void ThisTestAttributeFound()
        {
                var api = ApiReader.ReadApiFromProjects(m_ThisProjectFile.FullName, CancellationToken.None).Result;

                var thisTest = api.First().Members
                    .First(m => m.Name == nameof(AssemblyApiTests)).Members
                    .First(m => m.Name == nameof(SelfTests)).Members
                    .First(m => m.Name == nameof(ThisTestAttributeFound));

                Assert.That(thisTest.Attributes.Select(a => a.Key), Contains.Item(nameof(TestAttribute)));
        }

        private static void RunMain(string solutionPath, string outputFile, string inclusionRegexes, string exclusionRegexes)
        {
            Program.Main(new [] {solutionPath, outputFile, inclusionRegexes, exclusionRegexes});
        }
    }
}
