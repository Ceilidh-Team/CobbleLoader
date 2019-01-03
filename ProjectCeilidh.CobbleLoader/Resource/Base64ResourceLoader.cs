using System;
using System.IO;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public class Base64ResourceLoader : IResourceLoader
    {
        public bool CanAccept(Uri uri) => uri != null && uri.Scheme == "base64";

        public bool TryOpenStream(Uri uri, out Stream stream)
        {
            stream = default;

            if (!CanAccept(uri)) return false;

            try
            {
                stream = new MemoryStream(Convert.FromBase64String(uri.AbsolutePath));
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
