using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LylyraHelper.Code.Entities;

public class CustomPaper : Paper
{

    public CustomPaper(EntityData data, Vector2 offset) :
        base(data, offset,
            texture: "objects/LylyraHelper/dashPaper/dashpaper",
            gapTexture: "objects/LylyraHelper/dashPaper/dashpapergap")
    {

    }
}