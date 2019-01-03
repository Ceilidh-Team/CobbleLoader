using System.Collections.Generic;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.CobbleLoader.Loader
{
    public interface IModuleLoader
    {
        string LoaderId { get; }

        void LoadModules(IReadOnlyList<string> files, CobbleContext context);
    }
}
