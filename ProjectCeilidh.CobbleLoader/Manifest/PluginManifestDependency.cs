using System;
using System.Xml.Serialization;
using SemVer;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    /// <summary>
    /// Represents a plugin that's required to function
    /// </summary>
    [XmlRoot("dependency")]
    public class PluginManifestDependency
    {
        /// <summary>
        /// A Uri pointing to the manifest of the target plugin.
        /// This can be omitted, but will fail if the dependency isn't loaded
        /// by the time this plugin is processed
        /// </summary>
        [XmlAttribute("uri")]
        public string UriString { get; set; }

        [XmlIgnore]
        public Uri Uri => new Uri(UriString);

        /// <summary>
        /// The ID of the target plugin.
        /// Loading the manifest is skipped if this target already exists
        /// in the current context
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// A semver range describing acceptable plugin versions
        /// </summary>
        [XmlAttribute("version")]
        public string VersionString { get; set; }

        [XmlIgnore]
        public Range Range => new Range(VersionString);
    }
}
