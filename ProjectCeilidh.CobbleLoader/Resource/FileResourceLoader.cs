using System;
using System.IO;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public sealed class FileResourceLoader : IResourceLoader
    {
        public bool CanAccept(Uri uri) => uri != null && uri.Scheme == Uri.UriSchemeFile && uri.IsAbsoluteUri;

        public bool TryOpenStream(Uri uri, out Stream stream)
        {
            stream = default;

            if (!CanAccept(uri)) return false;

            try
            {
                stream = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }
    }
}
