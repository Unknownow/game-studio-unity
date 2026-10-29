using System;
using NUnit.Framework;
using Unknownow.StudioTools.Editor;

namespace Unknownow.StudioTools.Tests
{
    public class EvidenceCaptureTests
    {
        [TestCase("story-001")]
        [TestCase("a")]
        [TestCase("0day")]
        [TestCase("shop.panel_v2-final")]
        public void IsValidSlug_AcceptsLowerCaseSlugs(string slug)
        {
            Assert.IsTrue(EvidenceCapture.IsValidSlug(slug));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("-leading-dash")]
        [TestCase(".leading-dot")]
        [TestCase("Upper")]
        [TestCase("has space")]
        [TestCase("a/b")]
        [TestCase("..")]
        public void IsValidSlug_RejectsOtherValues(string slug)
        {
            Assert.IsFalse(EvidenceCapture.IsValidSlug(slug));
        }

        [Test]
        public void OutputPath_UsesSlugFolderAndTimestamp()
        {
            string path = EvidenceCapture.OutputPath("C:/game", "story-001", new DateTime(2026, 10, 29, 13, 5, 9));
            Assert.AreEqual("C:/game/production/qa/evidence/story-001/story-001-20261029-130509.png", path.Replace('\\', '/'));
        }

        [Test]
        public void TryParseSize_UsesFallbackWhenMissing()
        {
            Assert.IsTrue(EvidenceCapture.TryParseSize(new[] { "x" }, "-width", 1920, out int width));
            Assert.AreEqual(1920, width);
        }

        [TestCase("0")]
        [TestCase("-5")]
        [TestCase("abc")]
        public void TryParseSize_RejectsBadValues(string value)
        {
            Assert.IsFalse(EvidenceCapture.TryParseSize(new[] { "-width", value }, "-width", 1920, out int _));
        }
    }
}
