using System;
using System.IO;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public interface IResourceLoader
    {
        bool CanAccept(Uri uri);

        bool TryOpenStream(Uri uri, out Stream stream);
    }
}
