using System;
using System.Collections.Generic;
using System.IO;
using ProjectCeilidh.Cobble;
using ProjectCeilidh.CobbleLoader.Loader;
using ProjectCeilidh.CobbleLoader.Manifest;
using ProjectCeilidh.CobbleLoader.Resource;

namespace ProjectCeilidh.CobbleLoader
{
    public sealed class CobbleLoader
    {
        public CobbleContext Context { get; }
        public string PluginStorageDirectory { get; }

        private readonly Dictionary<string, PluginManifest> _manifests;
        private readonly CobbleContext _earlyCobbleContext;
        private readonly ResourceLoaderController _resourceLoaderController;
        private readonly ModuleLoaderController _moduleLoaderController;

        public CobbleLoader(CobbleContext context, string pluginStorageDir)
        {
            Context = context;
            PluginStorageDirectory = pluginStorageDir;

            _manifests = new Dictionary<string, PluginManifest>();
            _earlyCobbleContext = new CobbleContext();

            RegisterDefaultHandlers(_earlyCobbleContext);

            _earlyCobbleContext.Execute();

            if (!_earlyCobbleContext.TryGetSingleton(out _resourceLoaderController)) throw new Exception(); // TODO: Real exceptions
            if (!_earlyCobbleContext.TryGetSingleton(out _moduleLoaderController)) throw new Exception();

            Directory.CreateDirectory(pluginStorageDir);
        }

        private static void RegisterDefaultHandlers(CobbleContext context)
        {
            context.AddManaged<ResourceLoaderController>();
            context.AddManaged<FileResourceLoader>();
            context.AddManaged<FtpResourceLoader>();
            context.AddManaged<HttpResourceLoader>();
            context.AddManaged<Base64ResourceLoader>();

            context.AddManaged<ModuleLoaderController>();
            context.AddManaged<DotNetModuleLoader>();
        }
    }
}
