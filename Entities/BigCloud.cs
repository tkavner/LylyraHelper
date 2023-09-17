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

namespace Celeste.Mod.LylyraHelper.Entities
{
    [CustomEntity("LylyraHelper/BigCloud")]
    public class BigCloud : Cloud
    {
        private bool fragile;
        private DynData<Cloud> cloudData;
        private Sprite sprite;

        public BigCloud(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("fragile", true))
        {
            this.Collider.Width = 64;
            base.Collider.Position.X = -32f;
            base.Collider.Position.Y = -data.Int("offsetY", 5);
            this.fragile = data.Bool("fragile", true); //defaults to true because all BigClouds used to be fragile before adding option
            
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            cloudData = new DynData<Cloud>(this);
            // replace sprite-
            Remove(cloudData.Get<Sprite>("sprite"));
            sprite = LylyraHelperModule.SpriteBank.Create(fragile ? "fragileBigCloud" : "bigCloud");
            sprite.Play("idle", restart: false, randomizeFrame: false);
            sprite.CenterOrigin();
            cloudData["sprite"] = sprite;
            Add(sprite);
        }
    }
}