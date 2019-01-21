using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace ProjectCeilidh.CobbleLoader.Manifest
{
    public class PluginManifestFile
    {
        private static readonly SHA256CryptoServiceProvider Sha256Crypto = new SHA256CryptoServiceProvider();
        
        [XmlAttribute("uri")]
        public string Uri { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
        
        [XmlAttribute("sha256")]
        public string Sha256 { get; set; }
        
        [XmlAttribute("type")]
        public PluginManifestFileType Type { get; set; }

        public bool VerifyFile() => System.Uri.TryCreate(Uri, UriKind.Absolute, out _) &&
                                    (Name == null || !Name.Intersect(Path.GetInvalidFileNameChars()).Any());

        public bool VerifyHash(string filePath)
        {
            if (Sha256 == null) return true;

            byte[] targetHash;
            try
            {
                targetHash = Convert.FromBase64String(Sha256);
            }
            catch (FormatException)
            {
                return false;
            }
            
            using (var file = File.OpenRead(filePath))
                return new ReadOnlySpan<byte>(targetHash).SequenceEqual(new ReadOnlySpan<byte>(Sha256Crypto.ComputeHash(file)));
        }
        
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