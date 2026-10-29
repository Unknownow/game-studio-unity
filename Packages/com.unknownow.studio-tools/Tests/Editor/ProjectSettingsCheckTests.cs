using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unknownow.StudioTools.Editor;

namespace Unknownow.StudioTools.Tests
{
    public class ProjectSettingsCheckTests
    {
        const string Visible = "Visible Meta Files";

        static ProjectSettingsCheck.Check Find(ProjectSettingsCheck.Report report, string id)
        {
            return report.checks.Find(c => c.id == id);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(-1, false)]
        public void Evaluate_InputHandler_PassesForNewOrBoth(int handler, bool expected)
        {
            var report = ProjectSettingsCheck.Evaluate(SerializationMode.ForceText, Visible, handler);
            Assert.AreEqual(expected, Find(report, "input-system").pass);
        }

        [Test]
        public void Evaluate_FailsWhenNotForceText()
        {
            var report = ProjectSettingsCheck.Evaluate(SerializationMode.ForceBinary, Visible, 1);
            Assert.IsFalse(Find(report, "force-text").pass);
            Assert.AreEqual("ForceBinary", Find(report, "force-text").actual);
            Assert.IsFalse(report.AllPass);
        }

        [Test]
        public void Evaluate_FailsWhenMetaFilesAreHidden()
        {
            var report = ProjectSettingsCheck.Evaluate(SerializationMode.ForceText, "Hidden Meta Files", 1);
            Assert.IsFalse(Find(report, "visible-meta").pass);
        }

        [Test]
        public void Evaluate_AllGood_Passes()
        {
            var report = ProjectSettingsCheck.Evaluate(SerializationMode.ForceText, Visible, 2);
            Assert.IsTrue(report.AllPass);
            Assert.AreEqual(3, report.checks.Count);
        }

        [Test]
        public void ToJson_HasGeneratedAndChecks()
        {
            var report = ProjectSettingsCheck.Evaluate(SerializationMode.ForceText, Visible, 1);
            string json = ProjectSettingsCheck.ToJson(report);
            var parsed = JsonUtility.FromJson<ProjectSettingsCheck.Report>(json);
            Assert.IsNotEmpty(parsed.generated);
            Assert.AreEqual(3, parsed.checks.Count);
            Assert.AreEqual("force-text", parsed.checks[0].id);
            StringAssert.Contains("\"pass\"", json);
            StringAssert.Contains("\"actual\"", json);
        }

        [TestCase("  activeInputHandler: 0\n", 0)]
        [TestCase("a: 1\n  activeInputHandler: 2\nb: 3\n", 2)]
        [TestCase("nothing here", -1)]
        public void ParseActiveInputHandler_ReadsTheSetting(string text, int expected)
        {
            Assert.AreEqual(expected, ProjectSettingsCheck.ParseActiveInputHandler(text));
        }
    }
}
