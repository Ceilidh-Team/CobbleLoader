using System;
using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    /// <summary>
    /// Represents a file that will be processed during plugin retrieval
    /// </summary>
    public abstract class PluginManifestFile
    {
        /// <summary>
        /// A Uri that can be used to acquire the file
        /// </summary>
        [XmlAttribute("uri")]
        public string UriString { get; set; }

        [XmlIgnore]
        public Uri Uri => new Uri(UriString);

        /// <summary>
        /// A SHA256 hash of the target file
        /// </summary>
        [XmlAttribute("sha256")]
        public string HashString { get; set; }

        [XmlIgnore]
        public byte[] Hash => Convert.FromBase64String(HashString);
    }

    /// <summary>
    /// Represents a file that, once retrieved, will be fed to the specified loader
    /// </summary>
    [XmlRoot("loader")]
    public class PluginManifestLoaderFile : PluginManifestFile
    {

    }

    /// <summary>
    /// Represents a file that, once retrieved, will be added to a folder included in the PATH variable
    /// </summary>
    [XmlRoot("native")]
    public class PluginManifestNativeFile : PluginManifestFile
    {

    }

    /// <summary>
    /// Represents a file that, once retrieved, will be placed in the plugin's storage directory
    /// </summary>
    [XmlRoot("content")]
    public class PluginManifestContentFile : PluginManifestFile
    {

    }

    [XmlRoot("early")]
    public class PluginManifestEarlyFile : PluginManifestFile
    {

    }
}
