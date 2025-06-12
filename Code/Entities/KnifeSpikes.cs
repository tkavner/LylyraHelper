using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities;

[CustomEntity(new string[]
{
    "LylyraHelper/KnifeSpikesUp = LoadUp",
    "LylyraHelper/KnifeSpikesDown = LoadDown",
    "LylyraHelper/KnifeSpikesLeft = LoadLeft",
    "LylyraHelper/KnifeSpikesRight = LoadRight"
})]
public class KnifeSpikes : Spikes
{
    public bool sliceOnImpact { get; private set; }

    public int slicerLength;
    public string sliceableEntityTypes;

    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Up);
    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Down);
    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Left);
    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Right);
    public KnifeSpikes(EntityData data, Vector2 offset, Directions dir) : 
        base(data, offset, dir)
    {
        sliceOnImpact = data.Bool("sliceOnImpact", false);
        slicerLength = data.Int("slicerLength", 5);
        sliceableEntityTypes = data.Attr("sliceableEntityTypes", "");
    }
    public KnifeSpikes(Vector2 position, int size, Directions direction, string type, bool sliceOnImpact, int slicerLength, string sliceableEntityTypes) :
        base(position, size, direction, type)
    {
        this.sliceOnImpact = sliceOnImpact;
        this.slicerLength = slicerLength;
        this.sliceableEntityTypes = sliceableEntityTypes;
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);



        switch (Direction)
        {
            case Directions.Up:

                Collider cUp = new Hitbox(Width, slicerLength, 0, 0);
                cUp.BottomCenter = Collider.TopCenter;
                Add(new Slicer(-Vector2.UnitY, (int)Width, SceneAs<Level>(), slicerLength, cUp, sliceOnImpact: sliceOnImpact, settings: sliceableEntityTypes));
                break;

            case Directions.Down:

                Collider cDown = new Hitbox(Width, slicerLength, 0, 0);
                cDown.TopCenter = Collider.BottomCenter;
                Add(new Slicer(Vector2.UnitY, (int)Width, SceneAs<Level>(), slicerLength, cDown, sliceOnImpact: sliceOnImpact, settings: sliceableEntityTypes));
                break;
            case Directions.Left:

                Collider cLeft = new Hitbox(slicerLength, Height, 0, 0);
                cLeft.CenterRight = Collider.CenterLeft;
                Add(new Slicer(-Vector2.UnitX, (int)Height, SceneAs<Level>(), slicerLength, cLeft, sliceOnImpact: sliceOnImpact, settings: sliceableEntityTypes));
                break;
            case Directions.Right:

                Collider cRight = new Hitbox(slicerLength, Height, 0, 0);
                cRight.CenterLeft = Collider.CenterRight;
                Add(new Slicer(Vector2.UnitX, (int)Height, SceneAs<Level>(), slicerLength, cRight, sliceOnImpact: sliceOnImpact, settings: sliceableEntityTypes));
                break;
        }
    }

    private Vector2 VectorDirection()
    {
        switch (Direction)
        {
            case Directions.Right:
                return Vector2.UnitX;
            case Directions.Left:
                return -Vector2.UnitX;
            case Directions.Down:
                return Vector2.UnitY;
            default:
                return -Vector2.UnitY;
        }
    }
}