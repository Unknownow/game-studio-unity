using System.Collections.Generic;
using NUnit.Framework;
using Unknownow.StudioTools.Editor;

namespace Unknownow.StudioTools.Tests
{
    public class BatchTestsTests
    {
        [TestCase("EditMode", "EditMode")]
        [TestCase("PlayMode", "PlayMode")]
        [TestCase("All", "EditMode,PlayMode")]
        public void TryParsePlatforms_AcceptsKnownValues(string value, string expected)
        {
            Assert.IsTrue(BatchTests.TryParsePlatforms(new[] { "Unity", "-testPlatform", value }, out List<string> platforms, out string error));
            Assert.IsNull(error);
            Assert.AreEqual(expected, string.Join(",", platforms));
        }

        [Test]
        public void TryParsePlatforms_MissingArgument_Fails()
        {
            Assert.IsFalse(BatchTests.TryParsePlatforms(new[] { "Unity" }, out List<string> platforms, out string error));
            Assert.IsEmpty(platforms);
            StringAssert.Contains("-testPlatform is required", error);
        }

        [Test]
        public void TryParsePlatforms_MissingValue_Fails()
        {
            Assert.IsFalse(BatchTests.TryParsePlatforms(new[] { "Unity", "-testPlatform" }, out List<string> _, out string error));
            StringAssert.Contains("required", error);
        }

        [TestCase("editmode")]
        [TestCase("Standalone")]
        [TestCase("")]
        public void TryParsePlatforms_BadValue_Fails(string value)
        {
            Assert.IsFalse(BatchTests.TryParsePlatforms(new[] { "-testPlatform", value }, out List<string> _, out string error));
            StringAssert.Contains("must be EditMode, PlayMode or All", error);
        }

        [Test]
        public void ResultFileName_NamesTheFilePerPlatform()
        {
            Assert.AreEqual("editmode.xml", BatchTests.ResultFileName("EditMode"));
            Assert.AreEqual("playmode.xml", BatchTests.ResultFileName("PlayMode"));
        }

        [TestCase("Assets/InitTestScene119f2e7e-945d-435e-92ee-10aa700dd0c9.unity", true)]
        [TestCase("Assets/Scenes/InitTestSceneLobby.unity", false)]
        [TestCase("Assets/InitTestSceneLobby.unity", false)]
        [TestCase("Assets/Scenes/InitTestScene119f2e7e-945d-435e-92ee-10aa700dd0c9.unity", false)]
        public void IsInitTestScene_MatchesOnlyTheFrameworkScenes(string path, bool expected)
        {
            Assert.AreEqual(expected, BatchTests.IsInitTestScene(path));
        }
    }
}
