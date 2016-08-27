using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gtc.AssemblyApi;
using Gtc.AssemblyApi.CodeAnalysis;
using Gtc.AssemblyApi.Comparison;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.ModelBuilder;
using Gtc.AssemblyApi.SemVer;
using Gtc.AssemblyApiTests.Utils;
using NUnit.Framework;

namespace Gtc.AssemblyApiTests
{
    [TestFixture]
    public class RoundTripOwnApiTests
    {
        private readonly FileInfo m_ThisProjectFile;
        private readonly Lazy<IReadOnlyCollection<IApiNode>> m_LazyThisProjectApi;
        private readonly Lazy<IReadOnlyCollection<IApiNode>> m_LazyRoundTrippedProjectApi;

        public RoundTripOwnApiTests()
        {
            var solutionDirectory = new DirectoryInfo(Environment.CurrentDirectory + @"\..\..\");
            m_ThisProjectFile = solutionDirectory.GetFiles("*.csproj").First();
            m_LazyThisProjectApi = new Lazy<IReadOnlyCollection<IApiNode>>(() => ApiReader.ReadApiFromProjects(m_ThisProjectFile.FullName, CancellationToken.None).Result);
            m_LazyRoundTrippedProjectApi = new Lazy<IReadOnlyCollection<IApiNode>>(() => RoundTripApi(ThisProjectApi));
        }

        private IReadOnlyCollection<IApiNode> ThisProjectApi => m_LazyThisProjectApi.Value;
        private IReadOnlyCollection<IApiNode> RoundTrippedProjectApi => m_LazyRoundTrippedProjectApi.Value;


        private static IReadOnlyCollection<IApiNode> RoundTripApi(IReadOnlyCollection<IApiNode> originalApiNodes)
        {
            using (var tempFileManager = new TempFileManager())
            {
                var outputFile = tempFileManager.GetNew();
                JsonSerialization.WriteJson(originalApiNodes, outputFile);
                return JsonSerialization.ReadJson(outputFile);
            }
        }

        [Test]
        public void RoundTripSerializationOfOwnApi()
        {
            using (var tempFileManager = new TempFileManager())
            {
                var expectedContents = WriteHumanReadable(ThisProjectApi, tempFileManager);
                var expectedApi = File.ReadAllText(expectedContents.FullName);

                var deserializedApiNodes = RoundTrippedProjectApi;
                var actualContents = WriteHumanReadable(deserializedApiNodes, tempFileManager);

                var actualApi = File.ReadAllText(actualContents.FullName);
                Assert.That(actualApi, Is.EqualTo(expectedApi));
            }
        }

        [Test]
        public void ExactSameMembersIsBinaryIdentical()
        {
            var compatibility = GetApiCompatiblity(ThisProjectApi, RoundTrippedProjectApi);
            Assert.That(compatibility, Is.EqualTo(BinaryApiCompatibility.Identical));
        }

        [Test]
        public void AddingMembersIsBinaryBackwardsCompatible()
        {
                var publicRoundTrippedNodes = FilteredApiNode.For(new PrinterConfig("", ""), RoundTrippedProjectApi);

                var compatibility = GetApiCompatiblity(publicRoundTrippedNodes, ThisProjectApi);
                Assert.That(compatibility, Is.EqualTo(BinaryApiCompatibility.BackwardsCompatible));
        }

        [Test]
        public void RemovingMembersIsBinaryIncompatible()
        {
            var publicRoundTrippedNodes = FilteredApiNode.For(new PrinterConfig("", ""), RoundTrippedProjectApi);

            var compatibility = GetApiCompatiblity(ThisProjectApi, publicRoundTrippedNodes);
            Assert.That(compatibility, Is.EqualTo(BinaryApiCompatibility.Incompatible));
        }

        private BinaryApiCompatibility GetApiCompatiblity(IEnumerable<IApiNode> originalApi, IEnumerable<IApiNode> newApi)
        {
            var binaryApiComparer = new BinaryApiComparer();
            return binaryApiComparer.GetApiChangeType(ApiNodeComparison.Compare(originalApi, newApi));
        }

        private static FileInfo WriteHumanReadable(IEnumerable<IApiNode> apiNodes, TempFileManager tempFileManager)
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
                var humanReadableFile = tempFileManager.GetNew();
                var computerReadableFile = tempFileManager.GetNew();
                RunMain(m_ThisProjectFile, humanReadableFile, computerReadableFile, "TestCode;AssemblyApiTests", @"OurCode;AssemblyApi\.");
                var lines = File.ReadAllLines(humanReadableFile.FullName);

                var linesContainingThisMethod = lines.Where(l => l.Contains(nameof(AnalyzingSelfFindsThisMethodAndOtherStuff))).ToList();
                Assert.That(linesContainingThisMethod, Has.Count.EqualTo(1));

                Assert.That(lines, Has.Length.GreaterThan(15));
            }
        }

        [Test]
        public void ThisTestAttributeFound()
        {
                var api = ThisProjectApi;

                var thisTest = api.First().Members
                    .First(m => m.Name == nameof(AssemblyApiTests)).Members
                    .First(m => m.Name == nameof(RoundTripOwnApiTests)).Members
                    .First(m => m.Name == nameof(ThisTestAttributeFound));

                Assert.That(thisTest.Attributes.Keys, Contains.Item(nameof(TestAttribute)));
        }

        private static void RunMain(FileInfo solution, FileInfo humanReadableFile, FileInfo computerReadableFile, string inclusionRegexes, string exclusionRegexes)
        {
            Program.Main(new [] {solution.FullName, humanReadableFile.FullName, computerReadableFile.FullName, inclusionRegexes, exclusionRegexes});
        }
    }
}
