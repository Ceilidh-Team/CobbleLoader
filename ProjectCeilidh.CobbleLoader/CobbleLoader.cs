using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        public PluginInstallResult TryInstall(Uri manifestUri)
        {
            if (!TryLoadManifest(_resourceLoaderController, manifestUri, out var manifest))
                return new PluginInstallResult(null, PluginInstallResultCode.InvalidManifest);

            var installPath = Path.Combine(PluginStorageDirectory, manifest.Name);

            Directory.CreateDirectory(installPath);

            using (var manifestFile = File.Open(Path.Combine(installPath, "manifest.xml"), FileMode.Create))
                manifest.Serialize(manifestFile);

            foreach (var file in manifest.Files)
            {
                var fileUri = new Uri(file.Uri, UriKind.Absolute);
                if (!_resourceLoaderController.TryOpenStream(fileUri, out var stream))
                    return new PluginInstallResult(manifest, PluginInstallResultCode.DownloadFailure);

                var filePath = Path.Combine(installPath, file.Name ?? Path.GetFileName(fileUri.AbsolutePath));
                using (stream)
                using (var fileStream =
                    File.Open(filePath, FileMode.Create))
                    stream.CopyTo(fileStream);

                if (!file.VerifyHash(filePath))
                    return new PluginInstallResult(manifest, PluginInstallResultCode.ValidateFailure);
            }

            return new PluginInstallResult(manifest, PluginInstallResultCode.Success);
        }
        
        public async Task<PluginInstallResult> TryInstallAsync(Uri manifestUri)
        {
            if (!TryLoadManifest(_resourceLoaderController, manifestUri, out var manifest))
                return new PluginInstallResult(null, PluginInstallResultCode.InvalidManifest);

            var installPath = Path.Combine(PluginStorageDirectory, manifest.Name);

            Directory.CreateDirectory(installPath);

            using (var manifestFile = File.Open(Path.Combine(installPath, "manifest.xml"), FileMode.Create))
                manifest.Serialize(manifestFile);

            return new PluginInstallResult(manifest, (await Task.WhenAll(manifest.Files.Select(async file =>
            {
                var fileUri = new Uri(file.Uri, UriKind.Absolute);
                if (!_resourceLoaderController.TryOpenStream(fileUri, out var stream))
                    return PluginInstallResultCode.DownloadFailure;

                var filePath = Path.Combine(installPath, file.Name ?? Path.GetFileName(fileUri.AbsolutePath));
                using (stream)
                using (var fileStream =
                    File.Open(filePath, FileMode.Create))
                    await stream.CopyToAsync(fileStream);

                return file.VerifyHash(filePath) ? PluginInstallResultCode.Success : PluginInstallResultCode.ValidateFailure;
            }))).Aggregate(PluginInstallResultCode.Success, (a, b) => a != PluginInstallResultCode.Success ? a : b));
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
            return EnumerateInstalledPlugins().Select(manifest =>
            {
                if (manifest.Update == null)
                    return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.NotSupported);

                if (!TryLoadManifest(_resourceLoaderController, new Uri(manifest.Update, UriKind.Absolute),
                    out var updateManifest))
                    return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure);

                if (!PluginVersion.TryParse(manifest.Version, out var oldVersion) ||
                    !PluginVersion.TryParse(updateManifest.Version, out var newVersion) ||
                    manifest.Name != updateManifest.Name)
                    return new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure);

                if (oldVersion >= newVersion)
                    return new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.UpToDate);

                Uninstall(manifest.Name);

                var installRes = TryInstall(new Uri(manifest.Update, UriKind.Absolute));

                return installRes.ResultCode == PluginInstallResultCode.Success
                    ? new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.Success, installRes.ResultCode)
                    : new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.InstallFailure, installRes.ResultCode);
            });
        }
        
        public Task<PluginUpdateResult[]> UpdateAllAsync()
        {
            return Task.WhenAll(EnumerateInstalledPlugins().Select(manifest =>
            {
                if (manifest.Update == null)
                    return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.NotSupported));

                if (!TryLoadManifest(_resourceLoaderController, new Uri(manifest.Update, UriKind.Absolute),
                    out var updateManifest))
                    return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure));

                if (!PluginVersion.TryParse(manifest.Version, out var oldVersion) ||
                    !PluginVersion.TryParse(updateManifest.Version, out var newVersion) ||
                    manifest.Name != updateManifest.Name)
                    return Task.FromResult(new PluginUpdateResult(manifest.Name,
                        PluginUpdateResultCode.CheckFailure));

                return newVersion > oldVersion
                    ? UpdatePlugin()
                    : Task.FromResult(new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.UpToDate));

                async Task<PluginUpdateResult> UpdatePlugin()
                {
                    Uninstall(manifest.Name);

                    var installRes = await TryInstallAsync(new Uri(manifest.Update, UriKind.Absolute));

                    return installRes.ResultCode == PluginInstallResultCode.Success
                        ? new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.Success, installRes.ResultCode)
                        : new PluginUpdateResult(manifest.Name, PluginUpdateResultCode.InstallFailure, installRes.ResultCode);
                }
            }));
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
        
        public readonly struct PluginUpdateResult
        {
            public string PluginName { get; }
            public PluginUpdateResultCode ResultCode { get; }
            public PluginInstallResultCode? InstallResultCode { get; }

            internal PluginUpdateResult(string pluginName, PluginUpdateResultCode resultCode, PluginInstallResultCode? installResultCode = default)
            {
                PluginName = pluginName;
                ResultCode = resultCode;
                InstallResultCode = installResultCode;
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

        public readonly struct PluginInstallResult
        {
            public PluginManifest Manifest { get; }
            public PluginInstallResultCode ResultCode { get; }

            public PluginInstallResult(PluginManifest manifest, PluginInstallResultCode resultCode)
            {
                Manifest = manifest;
                ResultCode = resultCode;
            }
        }
        
        public enum PluginInstallResultCode
        {
            Success,
            InvalidManifest,
            DownloadFailure,
            ValidateFailure
        }
    }
}
