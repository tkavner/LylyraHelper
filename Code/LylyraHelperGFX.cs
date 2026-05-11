using Celeste.Mod.LylyraHelper.Other.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.LylyraHelper;

public static class LylyraHelperGFX
{
    public static Effect atmosphericWind;
    public static Effect ColorGradeDistort;
    
    public static void LoadContent()
    {
        atmosphericWind = EffectHelper.LoadEffect("atmosphericwind");
        ColorGradeDistort = EffectHelper.LoadEffect("DynamicColorGrading/colorGradeDistort");
    }

    public static void UnloadContent()
    {
        EffectHelper.DisposeAndSetNull(ref atmosphericWind);
        EffectHelper.DisposeAndSetNull(ref ColorGradeDistort);
    }
}