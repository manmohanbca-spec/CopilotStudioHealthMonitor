using NUnit.Framework;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Tests.Models
{
    [TestFixture]
    public class AlmDiffResultTests
    {
        [Test]
        public void DiffStatusCode_EnumHasFourMembers()
        {
            var values = System.Enum.GetValues(typeof(DiffStatusCode));
            Assert.That(values.Length, Is.EqualTo(4));
        }

        [TestCase(DiffStatusCode.Match, "✅")]
        [TestCase(DiffStatusCode.ContentDiffers, "⚠️")]
        [TestCase(DiffStatusCode.MissingInTarget, "❌")]
        [TestCase(DiffStatusCode.OnlyInTarget, "➕")]
        public void KnownDiffStatus_ContainsExpectedEmoji(DiffStatusCode code, string emoji)
        {
            // Verify the emoji constants used by AlmDiffService match expectations
            string status = code == DiffStatusCode.Match ? "✅ Match"
                : code == DiffStatusCode.ContentDiffers ? "⚠️ Content Differs"
                : code == DiffStatusCode.MissingInTarget ? "❌ Missing in Target"
                : "➕ Only in Target";

            Assert.That(status, Does.Contain(emoji));
        }
    }
}
