using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Triggers
{
    [Tracked(false)]
    [CustomEntity("LylyraHelper/AddSlicerTrigger")]
    public class AddSlicerTrigger : Trigger
    {
        private string direction;
        private bool sliceOnImpact;
        private bool oneUse;
        private bool fragile;
        private string[] entityTypes;
        private bool roomwide;
        private int knifeLength;
        private int cutSize;
        private bool onLoad;
        private bool used;

        public AddSlicerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            direction = data.Attr("direction", "All");
            sliceOnImpact = data.Bool("sliceOnImpact", false);
            oneUse = data.Bool("singleUse", false);
            fragile = data.Bool("fragileSlicer", false);
            entityTypes = data.Attr("entityTypes", "").Split(',');
            roomwide = data.Bool("roomwide", false);
            knifeLength = data.Int("slicerLength", 8);
            cutSize = data.Int("cutSize", 16);
            onLoad = data.Bool("onLoadOnly", false);
        }


        public override void Update()
        {
            base.Update(); 
            if (!used) foreach (Entity e in Scene.Entities)
            {
                if (e.Collider == null) continue;
                TryAddSlicer(e);

            }
            used = true;
        }

        public static void Load()
        {
            On.Monocle.Scene.Add_Entity += SlicerTriggerCheck;
        }
        public static void Unload()
        {
            On.Monocle.Scene.Add_Entity += SlicerTriggerCheck;
        }

        private static void SlicerTriggerCheck(On.Monocle.Scene.orig_Add_Entity orig, Scene self, Entity entity)
        {
            orig.Invoke(self, entity);
            foreach (AddSlicerTrigger trigger in self.Tracker.GetEntities<AddSlicerTrigger>())
            {
                if (entity.Collider == null && !trigger.oneUse)
                    trigger.TryAddSlicer(entity);
            }
        }

        private void TryAddSlicer(Entity entity)
        {
            string entityName = entity.GetType().FullName;
            bool flag0 = entityTypes.Length == 0;
            flag0 |= entityTypes.Contains(entityName);
            if (flag0)
            {
                bool tempCollide = this.Collidable;
                this.Collidable = true;
                bool collide = this.CollideCheck(entity);
                this.Collidable = tempCollide;
                if (collide || roomwide)
                {
                    Collider cUp = new Hitbox(entity.Collider.Width, entity.Collider.Height, -entity.Collider.Width / 2, -entity.Collider.Height / 2);
                    cUp.Height = cUp.Height + knifeLength;
                    cUp.Top -= knifeLength;

                    Collider cDown = new Hitbox(entity.Collider.Width, entity.Collider.Height, -entity.Collider.Width / 2, -entity.Collider.Height / 2);
                    cDown.Height = cDown.Height + knifeLength;

                    Collider cRight = new Hitbox(entity.Collider.Width, entity.Collider.Height, -entity.Collider.Width / 2, -entity.Collider.Height / 2);
                    cRight.Width = cRight.Width + knifeLength;

                    Collider cLeft = new Hitbox(entity.Collider.Width, entity.Collider.Height, -entity.Collider.Width / 2, -entity.Collider.Height / 2);
                    cLeft.Width = cLeft.Width + knifeLength;

                    cLeft.Left -= knifeLength;
                    if (direction == "All")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, cutSize, SceneAs<Level>(), knifeLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        entity.Add(new Slicer(Vector2.UnitY, cutSize, SceneAs<Level>(), knifeLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        entity.Add(new Slicer(Vector2.UnitX, cutSize, SceneAs<Level>(), knifeLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        entity.Add(new Slicer(-Vector2.UnitX, cutSize, SceneAs<Level>(), knifeLength, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile));

                        if (oneUse) break;
                    }
                    else if (direction == "Up")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, cutSize, SceneAs<Level>(), knifeLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        if (oneUse) break;
                    }
                    else if (direction == "Down")
                    {
                        entity.Add(new Slicer(Vector2.UnitY, cutSize, SceneAs<Level>(), knifeLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        if (oneUse) break;
                    }
                    else if (direction == "Right")
                    {
                        entity.Add(new Slicer(Vector2.UnitX, cutSize, SceneAs<Level>(), knifeLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        if (oneUse) break;
                    }
                    else if (direction == "Left")
                    {
                        entity.Add(new Slicer(-Vector2.UnitX, cutSize, SceneAs<Level>(), knifeLength, sliceOnImpact: sliceOnImpact, fragile: fragile));
                        if (oneUse) break;
                    }
                }
            }
        }
    }
}
