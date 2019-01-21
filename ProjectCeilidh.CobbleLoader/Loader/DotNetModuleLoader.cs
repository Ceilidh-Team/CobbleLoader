using System;
using System.Collections.Generic;
using System.IO;
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
                var asmLocation = Path.GetDirectoryName(file);
                
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
                
                var asm = Assembly.Load(File.ReadAllBytes(file));

                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;

                foreach (var type in asm.GetExportedTypes()
                    .Where(x => !x.IsAbstract && x.GetCustomAttribute<CobbleLoaderAttribute>() != null))
                    context.AddManaged(type);

                Assembly AssemblyResolve(object sender, ResolveEventArgs args)
                {
                    var depPath = Path.Combine(asmLocation, args.Name + ".dll");
                    
                    return File.Exists(depPath) ? Assembly.Load(File.ReadAllBytes(depPath)) : null;
                }
            }

            
        }
    }
}
