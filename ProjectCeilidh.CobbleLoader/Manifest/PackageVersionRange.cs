using System;
using System.Text.RegularExpressions;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    /// <summary>
    /// Represents a range of acceptable PackageVersions
    /// </summary>
    public readonly struct PackageVersionRange : IEquatable<PackageVersionRange>
    {
        /// <summary>
        /// A regular expression that matches valid version ranges.
        /// </summary>
        private static readonly Regex PackageVersionRangeRegex = new Regex(@"(?<equal>^\d+(?:\.\d+(?:\.(?:\d+|x))?)?$|^x(?:\.x(?:\.x)?)?$|^\d+(?:\.x(?:\.x)?)?$)|^(?<left>[(\[])(?:(?<min>\d+(?:\.\d+(?:\.\d+)?)?),(?<max>\d+(?:\.\d+(?:\.\d+)?)?)?|(?<min>\d+(?:\.\d+(?:\.\d+)?)?)?,(?<max>\d+(?:\.\d+(?:\.\d+)?)?))(?<right>[)\]])$");

        /// <summary>
        /// The minimum acceptable verion.
        /// </summary>
        public PackageVersion? MinimumVersion { get; }
        /// <summary>
        /// True if the lower bound is inclusive, false otherwise
        /// </summary>
        public bool MinimumInclusive { get; }
        /// <summary>
        /// The maximum acceptable version.
        /// </summary>
        public PackageVersion? MaximumVersion { get; }
        /// <summary>
        /// True if the upper bound is inclusive, false otherwise.
        /// </summary>
        public bool MaximumInclusive { get; }

        public PackageVersionRange(PackageVersion? minimumVersion, bool minimumInclusive,
            PackageVersion? maximumVersion, bool maximumInclusive)
        {
            if (!minimumVersion.HasValue && !maximumVersion.HasValue) throw new ArgumentException();

            MinimumVersion = minimumVersion;
            MinimumInclusive = minimumInclusive;
            MaximumVersion = maximumVersion;
            MaximumInclusive = maximumInclusive;
        }

        /// <summary>
        /// Determines if the given version is part of the range.
        /// </summary>
        /// <param name="version">The verison to test</param>
        /// <returns>True if <paramref name="version"/> is part of this range, false otherwise.</returns>
        public bool Includes(PackageVersion version)
        {
            bool minCondition;
            if (MinimumInclusive) minCondition = version >= (MinimumVersion ?? PackageVersion.MinValue);
            else minCondition = version > (MinimumVersion ?? PackageVersion.MinValue);

            bool maxCondition;
            if (MaximumInclusive) maxCondition = version <= (MaximumVersion ?? PackageVersion.MaxValue);
            else maxCondition = version < (MaximumVersion ?? PackageVersion.MaxValue);

            return minCondition && maxCondition;
        }

        public override bool Equals(object obj) =>
            obj is PackageVersionRange range && Equals(range);

        public bool Equals(PackageVersionRange other) =>
            MinimumVersion == other.MinimumVersion && MinimumInclusive == other.MinimumInclusive &&
            MaximumVersion == other.MaximumVersion && MaximumInclusive == other.MaximumInclusive;

        public override int GetHashCode() => (((MinimumVersion?.GetHashCode() ?? -1) << 8) ^ (MaximumVersion?.GetHashCode() ?? -1)) ^ (MinimumInclusive ? 0b10 : 0) ^ (MaximumInclusive ? 0b1 : 0);

        public override string ToString() => $"{(MinimumInclusive ? "[" : "(")}{MinimumVersion?.ToString(false) ?? ""},{MaximumVersion?.ToString(false) ?? ""}{(MaximumInclusive ? "]" : ")")}";

        public static bool operator ==(PackageVersionRange one, PackageVersionRange two) => one.Equals(two);
        public static bool operator !=(PackageVersionRange one, PackageVersionRange two) => !one.Equals(two);

        /// <summary>
        /// Convert a string to an equivalent PackageVersionRange.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted PackageVersionRange.</returns>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> cannot be converted.</exception>
        public static PackageVersionRange Parse(string value) =>
            TryParse(value, out var range) ? range : throw new FormatException();

        /// <summary>
        /// Attempt to convert a string to an equivalent PackageVersionRange.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="range">The converted PackageVersionRange.</param>
        /// <returns>True if the conversion succeeded, false otherwise.</returns>
        public static bool TryParse(string value, out PackageVersionRange range)
        {
            range = default;

            if (string.IsNullOrEmpty(value)) return false;

            var match = PackageVersionRangeRegex.Match(value);
            if (!match.Success) return false;

            if (match.Groups["equal"].Success)
            {
                if (!PackageVersion.TryParse(match.Groups["equal"].Value, out var equalVer)) return false;

                range = new PackageVersionRange(equalVer, true, new PackageVersion(equalVer.Major ?? int.MaxValue, equalVer.Minor ?? int.MaxValue, equalVer.Patch ?? int.MaxValue), true);
                return true;
            }

            var min = default(PackageVersion?);
            var max = default(PackageVersion?);
            if (match.Groups["min"].Success)
            {
                if (!PackageVersion.TryParse(match.Groups["min"].Value, out var minVer)) return false;
                min = minVer;
            }

            if (match.Groups["max"].Success)
            {
                if (!PackageVersion.TryParse(match.Groups["max"].Value, out var maxVer)) return false;
                max = maxVer;
            }

            range = new PackageVersionRange(min, match.Groups["left"].Value == "[", max, match.Groups["right"].Value == "]");
            return true;
        }
    }
}
