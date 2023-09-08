using MonoMod.ModInterop;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [ModImportName("FrostHelper")]
    public static class FrostHelperImports
    {
        public static Func<Type[], string> GetTypes;

        public static bool FrostHelperLoaded;

        public static void Load()
        {
            EverestModuleMetadata frostHelperMeta = new EverestModuleMetadata { Name = "FrostHelper", VersionString = "1.45.2" };
            
            if (Everest.Loader.DependencyLoaded(frostHelperMeta))
            {
                FrostHelperLoaded = true;

                typeof(FrostHelperImports).ModInterop();
                Logger.Log("LylyraHelper", "FrostHelper Found. Enabling FrostHelper API.");
            }
        }
    }
}