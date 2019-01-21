using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    public class PluginManifestLicense
    {
        [XmlAttribute("uri")]
        public string Uri { get; set; }
        
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}