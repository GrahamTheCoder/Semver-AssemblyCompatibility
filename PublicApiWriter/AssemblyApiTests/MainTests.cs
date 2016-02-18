using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssemblyApi;
using AssemblyApiTests.Utils;
using NUnit.Framework;

namespace AssemblyApiTests
{
    [TestFixture]
    public class MainTests
    {
        private readonly FileInfo m_SolutionFile;

        public MainTests()
        {
            var solutionDirectory = new DirectoryInfo(Environment.CurrentDirectory + @"\..\..\");
            m_SolutionFile = solutionDirectory.GetFiles("*.csproj").First();
        }

        [Test]
        public void AnalyzingSelfFindsThisMethodAndOtherStuff()
        {
            using (var tempFileManager = new TempFileManager())
            {
                var outputFile = tempFileManager.GetNew();
                RunMain(m_SolutionFile.FullName, outputFile.FullName, "TestCode;AssemblyApiTests", @"OurCode;AssemblyApi\.");
                var lines = File.ReadAllLines(outputFile.FullName);

                var linesContainingThisMethod = lines.Where(l => l.Contains(nameof(AnalyzingSelfFindsThisMethodAndOtherStuff))).ToList();
                Assert.That(linesContainingThisMethod, Has.Count.EqualTo(1));

                Assert.That(lines, Has.Length.GreaterThan(15));
            }
        }

        private static void RunMain(string solutionPath, string outputFile, string inclusionRegexes, string exclusionRegexes)
        {
            Program.Main(new [] {solutionPath, outputFile, inclusionRegexes, exclusionRegexes});
        }
    }
}
