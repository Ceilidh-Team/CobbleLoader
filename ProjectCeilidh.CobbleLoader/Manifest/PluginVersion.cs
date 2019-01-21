using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    /// <summary>
    /// Represents the version of a package.
    /// </summary>
    public readonly struct PluginVersion : IComparable, IComparable<PluginVersion>, IEquatable<PluginVersion>
    {
        /// <summary>
        /// A regular expression that matches all valid version strings
        /// </summary>
        private static readonly Regex PackageVersionRegex = new Regex(@"^(?<major>\d+)(?:\.(?<minor>\d+)(?:\.(?<patch>\d+|x))?)?$|^(?<major>x)(?:\.(?<minor>x)(?:\.(?<patch>x))?)?$|^(?<major>\d+)(?:\.(?<minor>x)(?:\.(?<patch>x))?)?$");

        /// <summary>
        /// The minimum possible PackageVersion value.
        /// </summary>
        public static readonly PluginVersion MinValue = new PluginVersion();
        /// <summary>
        /// The maximum possible PackageVersion value.
        /// </summary>
        public static readonly PluginVersion MaxValue = new PluginVersion(int.MaxValue, int.MaxValue, int.MaxValue);

        /// <summary>
        /// The major version of the package.
        /// Incremented whenever breaking API changes appear.
        /// </summary>
        public int? Major { get; }
        /// <summary>
        /// The minor version of the package.
        /// Incremented whenever non-breaking API changes appear.
        /// </summary>
        public int? Minor { get; }
        /// <summary>
        /// The patch version of the package.
        /// Incremented whenever changes appear that don't change the API.
        /// </summary>
        public int? Patch { get; }

        private PluginVersion(int? major, int? minor, int? patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Construct a PackageVersion from a standand version.
        /// </summary>
        /// <param name="version">The version to take numbers from</param>
        public PluginVersion(Version version) : this(version.Major < 0 ? default(int?) : version.Major, version.Minor < 0 ? default(int?) : version.Minor, version.Build < 0 ? default(int?) : version.Build)
        {

        }

        /// <summary>
        /// Construct a PackageVersion with a fixed major version, but wildcard minor and patch versions.
        /// </summary>
        /// <param name="major">The major version.</param>
        public PluginVersion(int major) : this(major, default(int?), default(int?))
        {
        }

        /// <summary>
        /// Construct a PackageVersion with a fixed major and minor version, but wildcard patch version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        public PluginVersion(int major, int minor) : this(major, minor, default(int?))
        {
        }

        /// <summary>
        /// Construct a fixed PackageVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        public PluginVersion(int major, int minor, int patch) : this(new int?(major), new int?(minor), new int?(patch))
        {
        }
        
        public int CompareTo(object obj) =>
            obj is PluginVersion ver ? CompareTo(ver) : throw new ArgumentException();

        public int CompareTo(PluginVersion other)
        {
            if ((Major ?? -1) > (other.Major ?? -1)) return 1;
            if (Major == other.Major)
            {
                if ((Minor ?? -1) > (other.Minor ?? -1)) return 1;
                if (Minor == other.Minor)
                {
                    if ((Patch ?? -1) > (other.Patch ?? -1)) return 1;
                    if (Patch == other.Patch) return 0;

                    return -1;
                }

                return -1;
            }

            return -1;
        }

        public void Deconstruct(out int? major, out int? minor, out int? patch)
        {
            major = Major;
            minor = Minor;
            patch = Patch;
        }

        public override bool Equals(object obj) =>
            obj is PluginVersion ver && Equals(ver);

        public bool Equals(PluginVersion ver) => CompareTo(ver) == 0;

        public override int GetHashCode() => (((Major ?? -1) & 0xFF) << 8 * 3) | (((Minor ?? -1) & 0xFF) << 8 * 2) | (Patch ?? -1) & 0xFFFF;

        public override string ToString() => ToString(true);

        public string ToString(bool showWildcard)
        {
            var builder = new StringBuilder();
            if (!Major.HasValue && showWildcard || Major.HasValue)
            {
                builder.Append(Major?.ToString() ?? "x");
                if (!Minor.HasValue && showWildcard || Minor.HasValue)
                {
                    builder.Append(".");
                    builder.Append(Minor?.ToString() ?? "x");
                    if (!Patch.HasValue && showWildcard || Patch.HasValue)
                    {
                        builder.Append(".");
                        builder.Append(Patch?.ToString() ?? "x");
                    }
                }
            }

            return builder.ToString();
        }

        public static explicit operator Version(PluginVersion packVer) => new Version(packVer.Major ?? -1, packVer.Minor ?? -1, packVer.Patch ?? -1);
        public static explicit operator PluginVersion(Version ver) => new PluginVersion(ver);

        public static bool operator <(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) < 0;

        public static bool operator >(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) > 0;

        public static bool operator <=(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) <= 0;

        public static bool operator >=(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) >= 0;

        public static bool operator ==(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) == 0;

        public static bool operator !=(PluginVersion version1, PluginVersion version2) =>
            version1.CompareTo(version2) != 0;

        /// <summary>
        /// Parse a given string and produce an equivalent PackageVersion.
        /// </summary>
        /// <param name="value">The string to be converted.</param>
        /// <returns>The PackageVersion for the given string.</returns>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> could not be parsed.</exception>
        public static PluginVersion Parse(string value) =>
            TryParse(value, out var version) ? version : throw new FormatException();

        /// <summary>
        /// Attempt to parse a given string and produce an equivalent PackageVersion.
        /// </summary>
        /// <param name="value">The string to be converted.</param>
        /// <param name="version">The PackageVersion for the given string.</param>
        /// <returns>True if conversion succeeded, false otherwise.</returns>
        public static bool TryParse(string value, out PluginVersion version)
        {
            version = default;

            if (string.IsNullOrEmpty(value)) return false;

            var match = PackageVersionRegex.Match(value);
            if (!match.Success) return false;

            if (!TryGetValue(match.Groups["major"], out var major)) return false;
            if (!TryGetValue(match.Groups["minor"], out var minor)) return false;
            if (!TryGetValue(match.Groups["patch"], out var patch)) return false;

            version = new PluginVersion(major, minor, patch);
            return true;

            bool TryGetValue(Group group, out int? groupVal)
            {
                groupVal = default;

                if (!group.Success) return true;
                if (group.Value == "x") return true;

                if (!int.TryParse(group.Value, out var tmp)) return false;

                groupVal = tmp;
                return true;
            }
        }
    }
}
