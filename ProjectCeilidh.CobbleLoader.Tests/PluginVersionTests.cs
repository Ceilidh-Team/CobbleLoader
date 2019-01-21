using System;
using ProjectCeilidh.CobbleLoader.Manifest;
using Xunit;

namespace ProjectCeilidh.CobbleLoader.Tests
{
    public class PluginVersionTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData(default(string), false)]
        [InlineData("1.0.0", true)]
        [InlineData("1.0", true)]
        [InlineData("1", true)]
        [InlineData("1.0.x", true)]
        [InlineData("1.x.x", true)]
        [InlineData("x.x.x", true)]
        [InlineData("x", true)]
        [InlineData("1.x.0", false)]
        [InlineData("x.0.x", false)]
        [InlineData("a", false)]
        [InlineData("1.0.0.0", false)]
        [InlineData("x.0", false)]
        public void ParseTest(string value, bool success)
        {
            Assert.Equal(success, PluginVersion.TryParse(value, out _));
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("1.1.0", "2.0.0", -1)]
        [InlineData("1.1.1", "2.0.0", -1)]
        [InlineData("2.0.0", "2.1.0", -1)]
        [InlineData("2.1.0", "2.1.1", -1)]
        [InlineData("2.1.1", "2.1.2", -1)]
        [InlineData("2.1.1", "2.2.1", -1)]
        [InlineData("1.3.2", "1.4.0", -1)]
        [InlineData("1.5.x", "1.4.2", 1)]
        [InlineData("1.5.x", "1.5.2", -1)]
        [InlineData("1.0.0", "1.0.0", 0)]
        public void CompareTest(string value1, string value2, int value)
        {
            Assert.True(PluginVersion.TryParse(value1, out var ver1));
            Assert.True(PluginVersion.TryParse(value2, out var ver2));

            Assert.Equal(value, Math.Sign(ver1.CompareTo(ver2)));
            Assert.Equal(-value, Math.Sign(ver2.CompareTo(ver1)));
        }

        [Theory]
        [InlineData(1, 5, 0)]
        [InlineData(1, 5, null)]
        [InlineData(1, null, null)]
        public void VersionToStringTest(int major, int? minor, int? patch)
        {
            var ver = MakeVersion(major, minor, patch);

            Assert.True(PluginVersion.TryParse(ver.ToString(true), out var wildcardVersion));
            Assert.True(PluginVersion.TryParse(ver.ToString(false), out var blankVersion));

            Assert.Equal(ver, wildcardVersion);
            Assert.Equal(ver, blankVersion);
        }

        [Theory]
        [InlineData(1, 5, 0, true, 1, 5, null, false)]
        [InlineData(1, 5, 0, true, 1, 5, null, true)]
        [InlineData(1, null, null, false, 1, 5, null, true)]
        public void RangeToStringTest(int minMajor, int? minMinor, int? minPatch, bool minInclusive, int maxMajor, int? maxMinor, int? maxPatch, bool maxInclusive)
        {
            var min = MakeVersion(minMajor, minMinor, minPatch);
            var max = MakeVersion(maxMajor, maxMinor, maxPatch);

            var range = new PluginVersionRange(min, minInclusive, max, maxInclusive);

            Assert.True(PluginVersionRange.TryParse(range.ToString(), out var reverseRange));

            Assert.Equal(range, reverseRange);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(default(string), false)]
        [InlineData("1.0.0", true)]
        [InlineData("1.0", true)]
        [InlineData("1", true)]
        [InlineData("1.0.x", true)]
        [InlineData("1.x.x", true)]
        [InlineData("x.x.x", true)]
        [InlineData("x", true)]
        [InlineData("1.x.0", false)]
        [InlineData("x.0.x", false)]
        [InlineData("a", false)]
        [InlineData("1.0.0.0", false)]
        [InlineData("x.0", false)]
        [InlineData("(1,0)", true)]
        [InlineData("(,0)", true)]
        [InlineData("(1,)", true)]
        [InlineData("[1,0)", true)]
        [InlineData("(1,0]", true)]
        [InlineData("1,0)", false)]
        [InlineData("(1.x,0)", false)]
        [InlineData("(,)", false)]
        public void RangeParseTest(string value, bool success)
        {
            Assert.Equal(success, PluginVersionRange.TryParse(value, out _));
        }

        [Theory]
        [InlineData("1.0.0", "1.0.0", true)]
        [InlineData("1.0.0", "2.0.0", false)]
        [InlineData("1.0.x", "1.0.5", true)]
        [InlineData("1.x.x", "1.5.2", true)]
        [InlineData("(1,)", "2.0.0", true)]
        [InlineData("(1,)", "0.1.0", false)]
        [InlineData("(1.0.0,)", "1.0.0", false)]
        [InlineData("[1.0.0,)", "1.0.0", true)]
        [InlineData("[1,2)", "1.5.0", true)]
        [InlineData("[1,2)", "2.0.0", false)]
        [InlineData("(,1)", "0.9.0", true)]
        [InlineData("(,1)", "1.0.0", false)]
        public void RangeIncludesTest(string rangeValue, string verValue, bool success)
        {
            Assert.True(PluginVersionRange.TryParse(rangeValue, out var range));
            Assert.True(PluginVersion.TryParse(verValue, out var ver));

            Assert.Equal(success, range.Includes(ver));
        }

        private static PluginVersion MakeVersion(int? major, int? minor, int? patch)
        {
            PluginVersion ver;

            if (!major.HasValue)
                ver = new PluginVersion();
            else if (!minor.HasValue)
                ver = new PluginVersion(major.Value);
            else
                ver = !patch.HasValue ? new PluginVersion(major.Value, minor.Value) : new PluginVersion(major.Value, minor.Value, patch.Value);

            return ver;
        }
    }
}
