using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities;

[CustomEntity("LylyraHelper/BigCloud")]
public class BigCloud : Cloud
{
    private bool fragile;
    private DynData<Cloud> cloudData;
    private Sprite sprite;

    private float timer;
    public BigCloud(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("fragile", true))
    {
        this.Collider.Width = 64;
        base.Collider.Position.X = -32f;
        base.Collider.Position.Y = -data.Int("offsetY", 5);
        this.fragile = data.Bool("fragile", true); //defaults to true because all BigClouds used to be fragile before adding option
        this.timer = Calc.Random.NextFloat() * 4f;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        cloudData = new DynData<Cloud>(this);
        // replace sprite-
        Remove(cloudData.Get<Sprite>("sprite"));
        sprite = LylyraHelperModule.SpriteBank.Create(fragile ? "fragilebigcloud" : "bigcloud");
        sprite.Play("idle", restart: false, randomizeFrame: false);
        sprite.CenterOrigin();
        cloudData["sprite"] = sprite;
        Add(sprite);
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
        if (GetPlayerRider() != null)
        {
            sprite.Position = Vector2.Zero;
        }
        else
        {
            sprite.Position = Calc.Approach(sprite.Position, new Vector2(0f, (float)Math.Sin(timer * 2f)) * 1.2F, Engine.DeltaTime * 4f); //the bigger sprite means we need a slightly different Approach()
        }
    }
}