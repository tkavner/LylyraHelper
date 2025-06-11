using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.LylyraHelper.Components;
using FMOD.Studio;
using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities;

[Tracked]
[CustomEntity("LylyraHelper/DashPaper")]
public class DashPaper : Paper
{

    private bool spawnScissors;
    private bool fragileScissors;
    private bool noTrail;
    private Random r = new Random(); //for particle gen only
    private int particleEmitPoints;
    private static ParticleType paperSymbols;
    private int playerParticleEmitPoints;
    private string sliceableEntityTypes;

    public DashPaper(EntityData data, Vector2 offset):
        base(data, offset,
            texture: "objects/LylyraHelper/dashPaper/dashpaper",
            gapTexture: "objects/LylyraHelper/dashPaper/dashpapergap")
    {
        thisType = this.GetType();
        this.spawnScissors = data.Bool("spawnScissors", true);
        this.fragileScissors = data.Bool("fragileScissors", false);
        this.sliceableEntityTypes = data.Attr("sliceableEntityTypes", "");
        noTrail = data.Bool("noTrail", false);
        noEffects = data.Bool("noEffects", false);
        if (paperSymbols == null)
        {
            Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                GFX.Game["particles/LylyraHelper/dashsymbol"],
                GFX.Game["particles/LylyraHelper/dashsymbol"],
                GFX.Game["particles/LylyraHelper/refillsymbol"],
                GFX.Game["particles/LylyraHelper/doublerefillsymbol"]);
            paperSymbols = new ParticleType()
            {
                SourceChooser = sourceChooser,
                Color = Color.White,
                Acceleration = new Vector2(0f, 0f),
                LifeMin = 0.4f,
                LifeMax = 1.2f,
                Size = .8f,
                SizeRange = 0.2f,
                SpeedMin = 0f,
                SpeedMax = 0f,
                RotationMode = ParticleType.RotationModes.SameAsDirection,
                ScaleOut = true,
                UseActualDeltaTime = true
            };
        }
    }

    public override void Update()
    {
        base.Update();
        if (!noEffects)
        {
            int i = r.Next(0, (int)Width / 8);
            int j = r.Next(0, (int)Height / 8);
            particleEmitPoints += Math.Min(100000, Math.Max(10000, (int)(Width * Height)));
            if (!skip[i, j] && particleEmitPoints > 200000)
            {
                particleEmitPoints -= 200000;
                SceneAs<Level>().ParticlesFG.Emit(paperSymbols, 1, Position + new Vector2(i * 8, j * 8), Vector2.Zero, Color.White);
            }

        }
    }

    internal override void OnDash(Vector2 direction)
    {
        if (Scene.Tracker.GetEntity<Player>().CollideCheck(this))
        {
            Activate(direction);
        }
    }

    internal virtual void Activate(Vector2 direction)
    {
        Player p = base.Scene.Tracker.GetEntity<Player>();
        var session = SceneAs<Level>().Session;
        session.Inventory.Dashes = p.MaxDashes;
        p.Dashes = p.MaxDashes;

        if (spawnScissors)
        {
            Vector2 gridPosition = new Vector2(8 * (int)(p.Position.X / 8), 8 * (int)(p.Position.Y / 8));

            Vector2 xOnly = new Vector2(direction.X, 0);
            Vector2 yOnly = new Vector2(0, direction.Y);
            if (direction.Y != 0)
            {
                var v1 = new Vector2(gridPosition.X, Position.Y + Height);
                var v2 = new Vector2(gridPosition.X, Position.Y + 0);
                Scissors s;
                if (direction.Y < 0)
                {
                    s = new Scissors([v1, v2], yOnly, fragileScissors, sliceableEntityTypes: sliceableEntityTypes);
                }
                else
                {
                    s = new Scissors([v2, v1], yOnly, fragileScissors, sliceableEntityTypes: sliceableEntityTypes);
                }
                base.Scene.Add(s);
            }
            if (direction.X != 0)
            {
                var v1 = new Vector2(Position.X + Width, gridPosition.Y);
                var v2 = new Vector2(Position.X, gridPosition.Y);
                Scissors s;
                if (direction.X < 0)
                {
                    s = new Scissors([v1, v2], xOnly, fragileScissors, sliceableEntityTypes: sliceableEntityTypes);
                }
                else
                {
                    s = new Scissors([v2, v1], xOnly, fragileScissors, sliceableEntityTypes: sliceableEntityTypes);
                }
                base.Scene.Add(s);
            }
        }
    }

    public bool CanActivate()
    {
        Player p = base.Scene.Tracker.GetEntity<Player>();
        if (this.CollideCheck<Player>())
        {

            Vector2[] playerPointsToCheck = new Vector2[] {
                (p.ExactPosition - this.Position),
                (p.ExactPosition + new Vector2(p.Width, 0) - this.Position),
                (p.ExactPosition + new Vector2(p.Width, -p.Height) - this.Position),
                (p.ExactPosition + new Vector2(0, -p.Height) - this.Position) };

            foreach (Vector2 v in playerPointsToCheck)
            {
                int x = (int)v.X;
                int y = (int)v.Y;
                if (x >= 0 && y >= 0 && x < (int)Width && y < (int)Height)
                {
                    if (!this.skip[x / 8, y / 8])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    internal override void AddPlayerEffects(Player player)
    {
        if (!noTrail && playerParticleEmitPoints++ % 5 == 0)
        {
            SceneAs<Level>().ParticlesFG.Emit(paperSymbols, 1, player.Center, player.Collider.HalfSize, Color.White);
        }
    }
}