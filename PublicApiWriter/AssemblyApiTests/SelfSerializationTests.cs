using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AssemblyApi;
using AssemblyApi.Comparison;
using AssemblyApi.ModelBuilder;
using AssemblyApi.Output;
using AssemblyApiTests.Utils;
using NUnit.Framework;

namespace AssemblyApiTests
{
    [TestFixture]
    public class SelfSerializationTests
    {
        private readonly FileInfo m_ThisProjectFile;

        public SelfSerializationTests()
        {
            var solutionDirectory = new DirectoryInfo(Environment.CurrentDirectory + @"\..\..\");
            m_ThisProjectFile = solutionDirectory.GetFiles("*.csproj").First();
        }


        [Test]
        public void RoundTripSerializationOfOwnApi()
        {
            using (var tempFileManager = new TempFileManager())
            {
                var outputFile = tempFileManager.GetNew();
                var originalApiNodes = ApiReader.ReadApiFromProjects(m_ThisProjectFile.FullName, CancellationToken.None).Result;
                var expectedContents = WriteHumanReadable(originalApiNodes, tempFileManager);
                var expectedApi = File.ReadAllText(expectedContents.FullName);

                JsonSerialization.WriteJson(originalApiNodes, outputFile);
                var deserializedApiNodes = JsonSerialization.ReadJson(outputFile);
                var actualContents = WriteHumanReadable(deserializedApiNodes, tempFileManager);

                var actualApi = File.ReadAllText(actualContents.FullName);
                Assert.That(actualApi, Is.EqualTo(expectedApi));
            }
        }

        [Test]
        public void RoundtrippedApiIsBinaryIdentical()
        {
            using (var tempFileManager = new TempFileManager())
            {
                var outputFile = tempFileManager.GetNew();
                var originalApiNodes = ApiReader.ReadApiFromProjects(m_ThisProjectFile.FullName, CancellationToken.None).Result;
                JsonSerialization.WriteJson(originalApiNodes, outputFile);
                var deserializedApiNodes = JsonSerialization.ReadJson(outputFile);

                var compatibility = GetApiCompatiblity(deserializedApiNodes, originalApiNodes);
                Assert.That(compatibility, Is.EqualTo(BinaryApiCompatibility.Identical));
            }
        }

        private BinaryApiCompatibility GetApiCompatiblity(IReadOnlyCollection<IApiNode> deserializedApiNodes, IReadOnlyCollection<IApiNode> originalApiNodes)
        {
            var binaryApiComparer = new BinaryApiComparer();
            return binaryApiComparer.GetApiChangeType(ApiNodeComparison.Compare(originalApiNodes, deserializedApiNodes));
        }

        private static FileInfo WriteHumanReadable(IReadOnlyCollection<IApiNode> apiNodes, TempFileManager tempFileManager)
        {
            var outputFile = tempFileManager.GetNew();
            new PublicApiWriter().WriteHumanReadable(apiNodes, outputFile, CancellationToken.None).Wait();
            return outputFile;
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
                    .First(m => m.Name == nameof(SelfSerializationTests)).Members
                    .First(m => m.Name == nameof(ThisTestAttributeFound));

                Assert.That(thisTest.Attributes.Keys, Contains.Item(nameof(TestAttribute)));
        }

        private static void RunMain(string solutionPath, string outputFile, string inclusionRegexes, string exclusionRegexes)
        {
            Program.Main(new [] {solutionPath, outputFile, inclusionRegexes, exclusionRegexes});
        }
    }
}
