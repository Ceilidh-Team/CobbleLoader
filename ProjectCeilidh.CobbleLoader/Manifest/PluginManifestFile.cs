using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    public class PluginManifestFile
    {
        [XmlAttribute("uri")]
        public string Uri { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
        
        [XmlAttribute("type")]
        public PluginManifestFileType Type { get; set; }

        public bool VerifyFile() => System.Uri.TryCreate(Uri, UriKind.Absolute, out _) &&
                                    (Name == null || !Name.Intersect(Path.GetInvalidFileNameChars()).Any());
        
        public enum PluginManifestFileType
        {
            [XmlEnum("loader")]
            Loader,
            [XmlEnum("early")]
            Early,
            [XmlEnum("content")]
            Content
        }
    }
}