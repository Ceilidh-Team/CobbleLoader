using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectCeilidh.Cobble;
using ProjectCeilidh.CobbleLoader.Attributes;

namespace ProjectCeilidh.CobbleLoader.Loader
{
    public class DotNetModuleLoader : IModuleLoader
    {
        public string LoaderId => "dotnet";

        public void LoadModules(IReadOnlyList<string> files, CobbleContext context)
        {
            foreach (var file in files)
            {
                var asm = Assembly.LoadFrom(file);

                foreach (var type in asm.GetExportedTypes()
                    .Where(x => x.GetCustomAttribute<CobbleLoaderAttribute>() != null))
                    context.AddManaged(type);
            }
        }
    }
}
