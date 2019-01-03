using System.Collections.Generic;
using System.Linq;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.CobbleLoader.Loader
{
    public class ModuleLoaderController : ILateInject<IModuleLoader>
    {
        private readonly Dictionary<string, IModuleLoader> _loaders;

        public ModuleLoaderController(IEnumerable<IModuleLoader> loaders)
        {
            _loaders = loaders.ToDictionary(x => x.LoaderId);
        }

        public bool TryGetModuleLoader(string loaderId, out IModuleLoader loader) => _loaders.TryGetValue(loaderId, out loader);

        public bool TryLoadModules(string loaderId, IReadOnlyList<string> files, CobbleContext context)
        {
            if (!TryGetModuleLoader(loaderId, out var loader)) return false;

            loader.LoadModules(files, context);
            return true;
        }

        public void UnitLoaded(IModuleLoader unit) => _loaders.Add(unit.LoaderId, unit);
    }
}
