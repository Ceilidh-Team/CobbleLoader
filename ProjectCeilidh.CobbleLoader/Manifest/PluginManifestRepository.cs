using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    public class PluginManifestRepository
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        
        [XmlAttribute("uri")]
        public string Uri { get; set; }
    }
}