using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.LylyraHelper.Code.Entities.SS2024;

[Tracked]
[CustomEntity(new string[] {
    "LylyraHelper/SS2024/WhimsyDoor = LoadVertical",
    "LylyraHelper/SS2024/WhimsyDoorVertical = LoadVertical",
    "LylyraHelper/SS2024/WhimsyDoorHorizontal = LoadHorizontal" })]
public class WhimsyDoor : Solid //aka SMWDoor
{
    private Orientations orientation;
    private MTexture[] doorTextures;
    private Sprite doorLock;
    public bool despawning;
    private bool renderChain = true;
    private Sprite[] chainSprites;

    public static Entity LoadVertical(Level level, LevelData levelData, Vector2 offset, EntityData data) => new WhimsyDoor(data, offset, Orientations.Vertical);
    public static Entity LoadHorizontal(Level level, LevelData levelData, Vector2 offset, EntityData data) => new WhimsyDoor(data, offset, Orientations.Horizontal);

    public enum Orientations { Vertical, Horizontal };

    public WhimsyDoor(Vector2 position, float width, float height, bool safe) :
        base(position, width, height, safe)
    {
    }

    public WhimsyDoor(EntityData data, Vector2 offset, Orientations ori) :
        base(
            data.Position + offset,
            ori == Orientations.Vertical ? 8 : data.Width,
            ori == Orientations.Vertical ? data.Height : 8,
            false)
    {
        orientation = ori;
        chainSprites = new Sprite[(int)Math.Max(Width / 8, Height / 8)];
        string string0 = ori == Orientations.Horizontal ? "smwDoorChainHBot" : "smwDoorChainBot";
        string string1 = ori == Orientations.Horizontal ? "smwDoorChainHTop" : "smwDoorChainTop";
        for (int i = 0; i < chainSprites.Length; i++)
        {
            Add(chainSprites[i] = LylyraHelperModule.SpriteBank.Create((i % 2 == 0) ? string1 : string0));
            chainSprites[i].Position = new Vector2(orientation == Orientations.Horizontal ? i * 8 + 4 : Width / 2, orientation == Orientations.Vertical ? i * 8 + 4 : Height / 2);
        }
        Add(doorLock = LylyraHelperModule.SpriteBank.Create(ori == Orientations.Horizontal ? "smwDoorLockH" : "smwDoorLock"));
        doorLock.CenterOrigin();
        if (ori == Orientations.Vertical) doorLock.Position = new Vector2(Width / 2, Height / 2);
        else doorLock.Position = new Vector2(Width / 2, Height / 2);
        Add(new ClimbBlocker(false));
        AllowStaticMovers = true;
        Depth = -500;
    }

    public WhimsyDoor(EntityData data, Vector2 offset) : this(data, offset, Orientations.Vertical)
    {
    }

    public void Open()
    {
        despawning = true; //add this here to ensure 1 key = 1 door openned
        Add(new Coroutine(OpenRoutine()));
    }

    private IEnumerator OpenRoutine()
    {
        doorLock.Play("open");
        foreach (var sm in staticMovers)
        {
            sm.Entity.Collidable = false;
        }
        int i = 0;
        foreach (Sprite sprite in chainSprites)
        {
            sprite.Play("open");
        }
        Collidable = false;
        yield return 0.2F;

        foreach (var sm in staticMovers)
        {
            Scene.Remove(sm.Entity);
        }
        foreach (Sprite sprite in chainSprites)
        {
            Remove(sprite);
            SceneAs<Level>().ParticlesBG.Emit(FinalBoss.P_Burst, 3, Position + sprite.Position, new Vector2(4, 4), Calc.HexToColor("ebaa77"),
                (float)(orientation == Orientations.Horizontal ? Math.PI / 2 + i++ % 2 * Math.PI : i++ % 2 * Math.PI));
        }
        yield return 1F;
        RemoveSelf();
        yield break;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

    }

}