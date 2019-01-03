using ProjectCeilidh.CobbleLoader.Manifest;

namespace ProjectCeilidh.CobbleLoader
{
    public sealed class PluginUpdateInformation
    {
        public UpdateScanResult ScanResult { get; }

        public PluginManifest OldManifest { get; }
        public PluginManifest NewManifest { get; }

        internal PluginUpdateInformation(UpdateScanResult result)
        {
            ScanResult = result;
        }

        internal PluginUpdateInformation(UpdateScanResult result, PluginManifest oldManifest, PluginManifest newManifest) : this(result)
        {
            OldManifest = oldManifest;
            NewManifest = newManifest;
        }

        public enum UpdateScanResult
        {
            Available,
            Failure,
            UpToDate
        }
    }
}
