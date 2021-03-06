using System;
using System.IO;
using System.Text;
using ProjectCeilidh.Cobble;
using Xunit;

namespace ProjectCeilidh.CobbleLoader.Tests
{
    public class CobbleLoaderTests
    {
        private const string MANIFEST_XML = @"<manifest
    name=""test-package""
    version=""1.0.0""
    homepage=""https://github.com/owner/project#readme""
    loader=""dotnet"">
    <description>A package used for testing CobbleLoader</description>
    <author name=""Olivia Trewin"" email=""trewinolivia@gmail.com"" url=""https://github.com/OrionNebula"" />
    <repository type=""git"" uri=""https://github.com/Ceilidh-Team/CobbleLoader""/>
    <license uri=""https://github.com/owner/project/LICENSE"" id=""MIT"" />
    <files>
        <file uri=""https://raw.githubusercontent.com/torvalds/linux/a7ddcea58ae22d85d94eabfdd3de75c3742e376b/README"" type=""content"" />
        <file uri=""base64:VEVTVCBURVhU"" name=""test.txt"" type=""content"" />
        <file uri=""base64:TVqQAAMAAAAEAAAA//8AALgAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAA4fug4AtAnNIbgBTM0hVGhpcyBwcm9ncmFtIGNhbm5vdCBiZSBydW4gaW4gRE9TIG1vZGUuDQ0KJAAAAAAAAABQRQAATAEDAPAcHtYAAAAAAAAAAOAAIiALATAAAAgAAAAIAAAAAAAAsicAAAAgAAAAQAAAAAAAEAAgAAAAAgAABAAAAAAAAAAEAAAAAAAAAACAAAAAAgAAAAAAAAMAQIUAABAAABAAAAAAEAAAEAAAAAAAABAAAAAAAAAAAAAAAF4nAABPAAAAAEAAAEgEAAAAAAAAAAAAAAAAAAAAAAAAAGAAAAwAAAAoJgAAVAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAACAAAAAAAAAAAAAAACCAAAEgAAAAAAAAAAAAAAC50ZXh0AAAAuAcAAAAgAAAACAAAAAIAAAAAAAAAAAAAAAAAACAAAGAucnNyYwAAAEgEAAAAQAAAAAYAAAAKAAAAAAAAAAAAAAAAAABAAABALnJlbG9jAAAMAAAAAGAAAAACAAAAEAAAAAAAAAAAAAAAAAAAQAAAQgAAAAAAAAAAAAAAAAAAAACSJwAAAAAAAEgAAAACAAUAYCAAAMgFAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABpyAQAAcCoiAigMAAAKACpCU0pCAQABAAAAAAAMAAAAdjQuMC4zMDMxOQAAAAAFAGwAAAC0AQAAI34AACACAACsAgAAI1N0cmluZ3MAAAAAzAQAAAwAAAAjVVMA2AQAABAAAAAjR1VJRAAAAOgEAADgAAAAI0Jsb2IAAAAAAAAAAgAAAUcWAAAJAAAAAPoBMwAWAAABAAAADgAAAAIAAAACAAAAAQAAAAwAAAALAAAAAQAAAAMAAAAAAHcBAQAAAAAABgDkACoCBgA2ASoCBgAoABcCDwBKAgAABgBTAF0BBgAdAeMBBgCvAOMBBgBsAOMBBgCJAOMBBgAEAeMBBgA8AOMBCgDOAFkCBgCiAqIBDgAKAIACAAAAAAEAAAAAAAEAAQABABAA0AGpATUAAQABAFAgAAAAAOYBVAEeAAEAVyAAAAAAhhgRAgYAAQACADkACQARAgEAEQARAgYAGQARAgoAKQARAhAAMQARAhAAOQARAhAAQQARAhAASQARAhAAUQARAhAAWQARAhAAYQARAgYAaQARAgYALgALACIALgATACsALgAbAEoALgAjAFMALgArAIkALgAzALUALgA7AMAALgBDAM0ALgBLAIkALgBTAIkAQwBbANgABIAAAAEAAAAAAAAAAAAAAAAAqQEAAAQAAgABAAAAAAAAABUAGQAAAAAAAQAAAAAAAAAAAAAAAAD1AQAAAAABAAAAAAAAAAAAAAAAAIACAAAAAAAAADxNb2R1bGU+AElUZXN0SW50ZXJmYWNlAFN5c3RlbS5SdW50aW1lAERlYnVnZ2FibGVBdHRyaWJ1dGUAQXNzZW1ibHlUaXRsZUF0dHJpYnV0ZQBUYXJnZXRGcmFtZXdvcmtBdHRyaWJ1dGUAQXNzZW1ibHlGaWxlVmVyc2lvbkF0dHJpYnV0ZQBBc3NlbWJseUluZm9ybWF0aW9uYWxWZXJzaW9uQXR0cmlidXRlAEFzc2VtYmx5Q29uZmlndXJhdGlvbkF0dHJpYnV0ZQBDb2JibGVMb2FkZXJBdHRyaWJ1dGUAQ29tcGlsYXRpb25SZWxheGF0aW9uc0F0dHJpYnV0ZQBBc3NlbWJseVByb2R1Y3RBdHRyaWJ1dGUAQXNzZW1ibHlDb21wYW55QXR0cmlidXRlAFJ1bnRpbWVDb21wYXRpYmlsaXR5QXR0cmlidXRlAEdldFZhbHVlAFN5c3RlbS5SdW50aW1lLlZlcnNpb25pbmcAUHJvamVjdENlaWxpZGguQ29iYmxlTG9hZGVyLlRlc3RQbHVnaW4uZGxsAFN5c3RlbQBQcm9qZWN0Q2VpbGlkaC5Db2JibGVMb2FkZXIuVGVzdFBsdWdpbgBUZXN0SW1wbGVtZW50YXRpb24AU3lzdGVtLlJlZmxlY3Rpb24AUHJvamVjdENlaWxpZGguQ29iYmxlTG9hZGVyAC5jdG9yAFN5c3RlbS5EaWFnbm9zdGljcwBTeXN0ZW0uUnVudGltZS5Db21waWxlclNlcnZpY2VzAERlYnVnZ2luZ01vZGVzAFByb2plY3RDZWlsaWRoLkNvYmJsZUxvYWRlci5BdHRyaWJ1dGVzAFByb2plY3RD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AQBBAHMAcwBlAG0AYgBsAHkAIABWAGUAcgBzAGkAbwBuAAAAMQAuADAALgAwAC4AMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAMAAAAtDcAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"" name=""ProjectCeilidh.CobbleLoader.TestPlugin.dll"" type=""loader"" sha256=""wWMnHSDDB1qvSdztmH3EQiXRmgvvdgZv0Y4QiFGXG1g="" />
    </files>
</manifest>";
        
        [Fact]
        public async void CobbleLoaderTest()
        {
            var context = new CobbleContext();
            var loader = new CobbleLoader(context, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
            Assert.True((await loader.TryInstallAsync(
                new Uri("base64:" + Convert.ToBase64String(Encoding.ASCII.GetBytes(MANIFEST_XML))))).ResultCode == CobbleLoader.PluginInstallResultCode.Success);
            Assert.Single(loader.EnumerateInstalledPlugins());
            Assert.True(File.ReadAllText(Path.Combine(loader.PluginStorageDirectory, "test-package", "README")).StartsWith("Linux kernel"));
            Assert.Equal("TEST TEXT", File.ReadAllText(Path.Combine(loader.PluginStorageDirectory, "test-package", "test.txt")));

            loader.LoadPlugins();
            
            await context.ExecuteAsync();
            Assert.True(context.TryGetSingleton(out ITestInterface test));
            Assert.Equal("test", test.GetValue());

            var updateRes = await loader.UpdateAllAsync();
            Assert.Single(updateRes);
            Assert.True(updateRes[0].ResultCode == CobbleLoader.PluginUpdateResultCode.NotSupported);
            
            loader.Uninstall("test-package");
            Assert.False(Directory.Exists(Path.Combine(loader.PluginStorageDirectory, "test-package")));
            Assert.Empty(loader.EnumerateInstalledPlugins());
        }
    }
}