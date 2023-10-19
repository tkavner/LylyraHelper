using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Code.Components;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Other;
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
        private List<string> entityTypes;
        private bool allTypes;
        private bool roomwide;
        private int slicerLength;
        private int cutSize;
        private bool onLoad;
        private bool used;
        private bool invert;
        private string sliceableEntityTypes;
        private string flag;

        public AddSlicerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            direction = data.Attr("direction", "All");
            sliceOnImpact = data.Bool("sliceOnImpact", false);
            oneUse = data.Bool("singleUse", false);
            fragile = data.Bool("fragileSlicer", false);
            roomwide = data.Bool("roomwide", false);
            slicerLength = data.Int("slicerLength", 8);
            cutSize = data.Int("cutSize", 16);
            onLoad = data.Bool("onLoadOnly", false);
            flag = data.Attr("flag", "");
            invert = data.Bool("invert", false);
            sliceableEntityTypes = data.Attr("sliceableEntityTypes", "");
            entityTypes = LyraUtils.GetFullNames(data.Attr("entityTypes", ""));
        }


        public override void Update()
        {
            base.Update();
            if (!used && CheckFlag())
            {
                foreach (Entity e in Scene.Entities)
                {
                    if (e.Collider == null) continue;
                    if (e.Get<Slicer>() != null) continue;
                    TryAddSlicer(e);
                }
            }
            if (onLoad) Scene.Remove(this);
        }

        public static void Load()
        {
            On.Monocle.Scene.Add_Entity += SlicerTriggerCheck;
        }
        public static void Unload()
        {
            On.Monocle.Scene.Add_Entity -= SlicerTriggerCheck;
        }

        public bool CheckFlag()
        {
            return flag == "" || (SceneAs<Level>().Session.GetFlag(flag) ^ invert);
        }

        private static void SlicerTriggerCheck(On.Monocle.Scene.orig_Add_Entity orig, Scene self, Entity entity)
        {
            orig.Invoke(self, entity);
            if (self == null || self.Tracker == null) return;
            foreach (AddSlicerTrigger trigger in self.Tracker.GetEntities<AddSlicerTrigger>())
            {
                if (trigger.used) continue;
                if (trigger.CheckFlag())
                {
                    if (entity.Collider == null) continue;
                    if (entity.Get<Slicer>() != null) continue;
                    if ((entity.Collider != null && entity.CollideCheck(trigger)))
                        trigger.TryAddSlicer(entity);
                }
            }
        }

        private void TryAddSlicer(Entity entity)
        {
            string entityName = entity.GetType().FullName;
            if (allTypes || entityTypes.Contains(entityName))
            {
                bool tempCollide = this.Collidable;
                this.Collidable = true;
                bool collide = this.CollideCheck(entity);
                this.Collidable = tempCollide;
                if (collide || roomwide)
                {
                    used = oneUse;
                    Collider cUp = new Hitbox(entity.Collider.Width, slicerLength, 0, 0);
                    cUp.BottomCenter = entity.Collider.TopCenter;

                    Collider cDown = new Hitbox(entity.Collider.Width, slicerLength, 0, 0);
                    cDown.TopCenter = entity.Collider.BottomCenter;

                    Collider cRight = new Hitbox(slicerLength, entity.Collider.Height, 0, 0);
                    cRight.CenterLeft = entity.Collider.CenterRight;

                    Collider cLeft = new Hitbox(slicerLength, entity.Collider.Height, 0, 0);
                    cLeft.CenterRight = entity.Collider.CenterLeft;
                    if (direction == "All")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, cutSize, SceneAs<Level>(), slicerLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile, settings:sliceableEntityTypes));
                        entity.Add(new Slicer(Vector2.UnitY, cutSize, SceneAs<Level>(), slicerLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                        entity.Add(new Slicer(Vector2.UnitX, cutSize, SceneAs<Level>(), slicerLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                        entity.Add(new Slicer(-Vector2.UnitX, cutSize, SceneAs<Level>(), slicerLength, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Up")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, cutSize, SceneAs<Level>(), slicerLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Down")
                    {
                        entity.Add(new Slicer(Vector2.UnitY, cutSize, SceneAs<Level>(), slicerLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Right")
                    {
                        entity.Add(new Slicer(Vector2.UnitX, cutSize, SceneAs<Level>(), slicerLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Left")
                    {
                        entity.Add(new Slicer(-Vector2.UnitX, cutSize, SceneAs<Level>(), slicerLength, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                }
            }
        }
    }
}
