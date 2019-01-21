using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    [XmlRoot("manifest")]
    public class PluginManifest
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(PluginManifest));
        internal const string DEFAULT_LOADER = "dotnet";
        
        [XmlAttribute("name")]
        public string Name { get; set; }
        
        [XmlAttribute("version")]
        public string Version { get; set; }
        
        [XmlAttribute("homepage")]
        public string Homepage { get; set; }
        
        [XmlAttribute("loader")]
        public string Loader { get; set; }
        
        [XmlAttribute("update")]
        public string Update { get; set; }
        
        [XmlElement("description")]
        public string Description { get; set; }
        
        [XmlElement("author")]
        public PluginManifestPerson Author { get; set; }
        
        [XmlArray("files")]
        [XmlArrayItem("file")]
        public PluginManifestFile[] Files { get; set; }

        public bool VerifyManifest() => !Name.Intersect(Path.GetInvalidFileNameChars()).Any()
                                        && PluginVersion.TryParse(Version, out _)
                                        && (Homepage == null || Uri.TryCreate(Homepage, UriKind.Absolute, out _))
                                        && (Update == null || Uri.TryCreate(Update, UriKind.Absolute, out _))
                                        && Files.All(x => x.VerifyFile());

        internal void Serialize(Stream stream) => XmlSerializer.Serialize(stream, this);

        internal static bool TryDeserialize(Stream stream, out PluginManifest manifest)
        {
            manifest = default;
            
            try
            {
                manifest = (PluginManifest) XmlSerializer.Deserialize(stream);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
