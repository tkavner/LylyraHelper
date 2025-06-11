using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached;

public class AttachedSpringSliceable : AttachedSliceableComponent
{
    public AttachedSpringSliceable() { }

    public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
    {
        Spring spring = original as Spring;
        if (desiredLength < 16) return null;
        switch (spring.Orientation)
        {
            case Spring.Orientations.WallRight:
                return new Spring(position, Spring.Orientations.WallRight, true);
            case Spring.Orientations.WallLeft:
                return new Spring(position, Spring.Orientations.WallLeft, true);
            case Spring.Orientations.Floor:
                return new Spring(position, Spring.Orientations.Floor, true);
            default:
                return null;

        }
    }

    public override string GetOrientation(Entity orientableEntity)
    {
        Spring spring = orientableEntity as Spring;
        switch (spring.Orientation)
        {
            case Spring.Orientations.WallRight:
                return "left"; //right wall of room, meaning left attatched
            case Spring.Orientations.WallLeft:
                return "right"; //left wall of room, meaning right attached.
            case Spring.Orientations.Floor:
                return "up";
            default:
                return "";
        }

    }
}