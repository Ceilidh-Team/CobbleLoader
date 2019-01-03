using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public sealed class ResourceLoaderController : ILateInject<IResourceLoader>
    {
        private readonly ICollection<IResourceLoader> _resourceLoaders;

        public ResourceLoaderController(IEnumerable<IResourceLoader> resourceLoaders)
        {
            _resourceLoaders = resourceLoaders.ToList();
        }

        public bool CanAccept(Uri uri) => _resourceLoaders.SingleOrDefault(x => x.CanAccept(uri)) != null;

        public bool TryOpenStream(Uri uri, out Stream stream)
        {
            stream = default;

            var loader = _resourceLoaders.SingleOrDefault(x => x.CanAccept(uri));

            return loader != default && loader.TryOpenStream(uri, out stream);
        }

        public void UnitLoaded(IResourceLoader unit) => _resourceLoaders.Add(unit);
    }
}
