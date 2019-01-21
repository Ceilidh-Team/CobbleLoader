using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    public class PluginManifestPerson
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        
        [XmlAttribute("email")]
        public string Email { get; set; }
        
        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}