using Microsoft.Xna.Framework;

namespace Celeste.Mod.LylyraHelper.Intefaces;

public interface ICuttable
{
    bool Cut(Vector2 cutPosition, Vector2 direction, int gapWidth, Vector2 cutStartPosition);


}