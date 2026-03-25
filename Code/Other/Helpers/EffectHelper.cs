using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Other.Helpers;

public static class EffectHelper
{
    private const string LogID = $"{nameof(LylyraHelper)}/{nameof(EffectHelper)}";
    
    public static Effect LoadEffect(string id)
    {
        string path = $"LylyraHelper:/Effects/LylyraHelper/{id}.cso";

        if (Everest.Content.TryGet(path, out ModAsset effect))
        {
            Logger.Info(LogID, $"Loaded effect from {path}.");
            return new Effect(Engine.Graphics.GraphicsDevice, effect.Data);
            
        }

        Logger.Error(LogID, $"Failed to find effect at {path}!");
        return null;
    }
    public static void DisposeAndSetNull(ref Effect effect)
    {
        effect?.Dispose();
        effect = null;
    }
    
    public static void DisposeAndSetNull(ref Atlas atlas)
    {
        atlas?.Dispose();
        atlas = null;
    }
}