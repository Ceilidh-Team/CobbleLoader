using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly CobbleContext _earlyCobbleContext;
        private readonly ResourceLoaderController _resourceLoaderController;
        private readonly ModuleLoaderController _moduleLoaderController;
        private readonly DotNetModuleLoader _dotNetModuleLoader;

        public CobbleLoader(CobbleContext context, string pluginStorageDir)
        {
            Context = context;
            PluginStorageDirectory = pluginStorageDir;

            _earlyCobbleContext = new CobbleContext();

            RegisterDefaultHandlers(_earlyCobbleContext);

            _earlyCobbleContext.Execute();

            if (!_earlyCobbleContext.TryGetSingleton(out _resourceLoaderController)) throw new Exception(); // TODO: Real exceptions
            if (!_earlyCobbleContext.TryGetSingleton(out _moduleLoaderController)) throw new Exception();
            if (!_earlyCobbleContext.TryGetSingleton(out _dotNetModuleLoader)) throw new Exception();

            Directory.CreateDirectory(pluginStorageDir);
        }

        public void Uninstall(string packageName)
        {
            Directory.Delete(Path.Combine(PluginStorageDirectory, packageName), true);
        }
        
        public bool TryInstall(Uri manifestUri)
        {
            if (!TryLoadManifest(_resourceLoaderController, manifestUri, out var manifest)) return false;

            var installPath = Path.Combine(PluginStorageDirectory, manifest.Name);

            Directory.CreateDirectory(installPath);

            using (var manifestFile = File.Open(Path.Combine(installPath, "manifest.xml"), FileMode.Create))
                manifest.Serialize(manifestFile);

            foreach (var file in manifest.Files)
            {
                var fileUri = new Uri(file.Uri, UriKind.Absolute);
                if (!_resourceLoaderController.TryOpenStream(fileUri, out var stream))
                    return false;

                using (stream)
                using (var fileStream =
                    File.Open(Path.Combine(installPath, file.Name ?? Path.GetFileName(fileUri.AbsolutePath)),
                        FileMode.Create))
                    stream.CopyTo(fileStream);
            }

            return true;
        }
        
        public async Task<bool> TryInstallAsync(Uri manifestUri)
        {
            if (!TryLoadManifest(_resourceLoaderController, manifestUri, out var manifest)) return false;

            var installPath = Path.Combine(PluginStorageDirectory, manifest.Name);

            Directory.CreateDirectory(installPath);

            using (var manifestFile = File.Open(Path.Combine(installPath, "manifest.xml"), FileMode.Create))
                manifest.Serialize(manifestFile);

            return (await Task.WhenAll(manifest.Files.Select(async file =>
            {
                var fileUri = new Uri(file.Uri, UriKind.Absolute);
                if (!_resourceLoaderController.TryOpenStream(fileUri, out var stream))
                    return false;

                using (stream)
                using (var fileStream =
                    File.Open(Path.Combine(installPath, file.Name ?? Path.GetFileName(fileUri.AbsolutePath)),
                        FileMode.Create))
                    await stream.CopyToAsync(fileStream);

                return true;
            }))).Aggregate(true, (a, b) => a && b);
    }
        
        public void LoadPlugins()
        {
            var earlyFiles = new List<string>();
            var loaderFiles = new Dictionary<string, List<string>>();

            foreach (var manifest in EnumerateInstalledPlugins())
            {
                var basePath = Path.Combine(PluginStorageDirectory, manifest.Name);
                
                if (!loaderFiles.TryGetValue(manifest.Loader, out var loaderList))
                    loaderList = loaderFiles[manifest.Loader] = new List<string>();

                foreach (var manifestFile in manifest.Files)
                {
                    var filePath = Path.Combine(basePath,
                        manifestFile.Name ??
                        Path.GetFileName(new Uri(manifestFile.Uri, UriKind.Absolute).AbsolutePath));

                    switch (manifestFile.Type)
                    {
                        case PluginManifestFile.PluginManifestFileType.Early:
                            earlyFiles.Add(filePath);
                            break;
                        case PluginManifestFile.PluginManifestFileType.Loader:
                            loaderList.Add(filePath);
                            break;
                    }
                }
            }

            _dotNetModuleLoader.LoadModules(earlyFiles, _earlyCobbleContext);

            foreach (var (loaderId, files) in loaderFiles)
            {
                if (!_moduleLoaderController.TryGetModuleLoader(loaderId, out var loader)) throw new Exception();

                loader.LoadModules(files, Context);
            }
        }

        public IEnumerable<PluginManifest> EnumerateInstalledPlugins()
        {
            foreach (var pluginDir in Directory.EnumerateDirectories(PluginStorageDirectory))
            {
                var manifestPath = Path.Combine(pluginDir, "manifest.xml");
                
                if (!File.Exists(manifestPath)) continue;

                PluginManifest manifest;
                using (var manifestStream = File.OpenRead(manifestPath))
                    if (!PluginManifest.TryDeserialize(manifestStream, out manifest)) continue;
                yield return manifest;
            }
        }
        
        public IEnumerable<PluginUpdateResult> UpdateAll()
        {
            foreach (var manifest in EnumerateInstalledPlugins())
            {
                if (manifest.Update == null)
                {
                    yield return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.NotSupported);
                    continue;
                }

                if (!TryLoadManifest(_resourceLoaderController, new Uri(manifest.Update, UriKind.Absolute),
                    out var updateManifest))
                {
                    yield return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure);
                    continue;
                }

                if (!PluginVersion.TryParse(manifest.Version, out var oldVersion) ||
                    !PluginVersion.TryParse(updateManifest.Version, out var newVersion) ||
                    oldVersion >= newVersion)
                {
                    yield return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.UpToDate);
                    continue;
                }

                Uninstall(manifest.Name);

                yield return !TryInstall(new Uri(manifest.Update, UriKind.Absolute))
                    ? new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.InstallFailure)
                    : new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.Success);
            }
        }
        
        public IEnumerable<Task<PluginUpdateResult>> UpdateAllAsync()
        {
            foreach (var manifest in EnumerateInstalledPlugins())
            {
                if (manifest.Update == null)
                {
                    yield return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.NotSupported));
                    continue;
                }

                if (!TryLoadManifest(_resourceLoaderController, new Uri(manifest.Update, UriKind.Absolute),
                    out var updateManifest))
                {
                    yield return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure));
                    continue;
                }

                if (!PluginVersion.TryParse(manifest.Version, out var oldVersion) ||
                    !PluginVersion.TryParse(updateManifest.Version, out var newVersion) ||
                    oldVersion >= newVersion)
                {
                    yield return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.UpToDate));
                    continue;
                }

                yield return UpdatePlugin(); 

                async Task<PluginUpdateResult> UpdatePlugin()
                {
                    Uninstall(manifest.Name);

                    return !await TryInstallAsync(new Uri(manifest.Update, UriKind.Absolute))
                        ? new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.InstallFailure)
                        : new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.Success);
                }
            }
        }

        private static bool TryLoadManifest(ResourceLoaderController res, Uri manifestUri, out PluginManifest manifest)
        {
            manifest = default;

            if (!res.TryOpenStream(manifestUri, out var stream)) return false;

            using (stream)
                return PluginManifest.TryDeserialize(stream, out manifest) && manifest.VerifyManifest();
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
        
        public struct PluginUpdateResult
        {
            public string PluginName { get; }
            public PluginUpdateResultCode ResultCode { get; }

            internal PluginUpdateResult(string pluginName, PluginUpdateResultCode resultCode)
            {
                PluginName = pluginName;
                ResultCode = resultCode;
            }
        }

        public enum PluginUpdateResultCode
        {
            Success,
            NotSupported,
            CheckFailure,
            InstallFailure,
            UpToDate
        }
    }
}
