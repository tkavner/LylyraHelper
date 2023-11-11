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
        private float pluginVersion;
        private bool allTypes;
        private bool roomwide;
        private int slicerLength;
        private int minimumCutSize;
        private bool onLoad;
        private bool used;
        private bool invert;
        private bool cutSizeMatching;
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
            onLoad = data.Bool("onLoadOnly", false);
            flag = data.Attr("flag", "");
            invert = data.Bool("invert", false);
            cutSizeMatching = data.Bool("matchCutSize", false);
            sliceableEntityTypes = data.Attr("sliceableEntityTypes", "");
            entityTypes = LyraUtils.GetFullNames(data.Attr("entityTypes", ""));
            pluginVersion = data.Float("pluginVersion");
            if (pluginVersion >= 2) minimumCutSize = data.Int("minimumCutSize", 16);
            else
            {
                minimumCutSize = data.Int("cutSize", 16);
            }
        }


        public override void Update()
        {
            base.Update();
            if (!used && CheckFlag())
            {
                foreach (Entity e in Scene.Entities)
                {
                    if (e.Collider == null) continue;
                    if (HasMatchingDirectionalSlicer(e)) continue;
                    TryAddSlicer(e);
                }
            }
            if (onLoad) Scene.Remove(this);
        }

        public static void Load()
        {
            On.Monocle.Entity.Awake += SlicerTriggerCheck;
        }
        public static void Unload()
        {
            On.Monocle.Entity.Awake -= SlicerTriggerCheck;
        }

        private static void SlicerTriggerCheck(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {

            orig.Invoke(self, scene);
            if (scene == null || scene.Tracker == null) return;
            if (!scene.Tracker.IsEntityTracked<AddSlicerTrigger>()) return; //this stops load crash bugs apparently
            foreach (AddSlicerTrigger trigger in scene.Tracker.GetEntities<AddSlicerTrigger>())
            {
                if (trigger.used) continue;
                if (trigger.CheckFlag())
                {
                    if (self.Collider == null) continue;
                    if (trigger.HasMatchingDirectionalSlicer(self)) continue;
                    if ((self.Collider != null && self.CollideCheck(trigger)))
                        trigger.TryAddSlicer(self);

                }
            }
        }

        private bool HasMatchingDirectionalSlicer(Entity entity)
        {
            foreach (Component component in entity.Components) {
                if (component is Slicer slicer)
                {
                    if (pluginVersion > 0)
                    {
                        if (slicer.Direction == DirectionalVector() && pluginVersion > 0) return true;
                    }
                    else return true;
                }
            }
            return false;
        }

        private Vector2 DirectionalVector()
        {
            switch(direction.ToLower())
            {
                case "up":
                    return -Vector2.UnitY;
                case "down":
                    return Vector2.UnitY;
                case "right":
                    return Vector2.UnitX;
                case "left":
                    return -Vector2.UnitX;
            }
            return Vector2.Zero;
        }

        public bool CheckFlag()
        {
            return flag == "" || (SceneAs<Level>().Session.GetFlag(flag) ^ invert);
        }

        private static void SlicerTriggerCheck(On.Monocle.Scene.orig_Add_Entity orig, Scene self, Entity entity)
        {
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
                    Collider cUp = new Hitbox(entity.Width, slicerLength, 0, 0);
                    cUp.BottomCenter = entity.Collider.TopCenter;

                    Collider cDown = new Hitbox(entity.Width, slicerLength, 0, 0);
                    cDown.TopCenter = entity.Collider.BottomCenter;

                    Collider cRight = new Hitbox(slicerLength, entity.Height, 0, 0);
                    cRight.CenterLeft = entity.Collider.CenterRight;

                    Collider cLeft = new Hitbox(slicerLength, entity.Height, 0, 0);
                    cLeft.CenterRight = entity.Collider.CenterLeft;

                    int horizCutSize = (int)Math.Max(entity.Width, minimumCutSize);
                    int vertCutSize = (int)Math.Max(entity.Height, minimumCutSize);
                    if (pluginVersion < 2)
                    {
                        horizCutSize = minimumCutSize;
                        vertCutSize = minimumCutSize;
                    }

                    if (direction == "All")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, horizCutSize, SceneAs<Level>(), slicerLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile, settings:sliceableEntityTypes));
                        entity.Add(new Slicer(Vector2.UnitY, horizCutSize, SceneAs<Level>(), slicerLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                        entity.Add(new Slicer(Vector2.UnitX, vertCutSize, SceneAs<Level>(), slicerLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                        entity.Add(new Slicer(-Vector2.UnitX, vertCutSize, SceneAs<Level>(), slicerLength, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Up")
                    {
                        entity.Add(new Slicer(-Vector2.UnitY, horizCutSize, SceneAs<Level>(), slicerLength, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Down")
                    {
                        entity.Add(new Slicer(Vector2.UnitY, horizCutSize, SceneAs<Level>(), slicerLength, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Right")
                    {
                        entity.Add(new Slicer(Vector2.UnitX, vertCutSize, SceneAs<Level>(), slicerLength, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                    else if (direction == "Left")
                    {
                        entity.Add(new Slicer(-Vector2.UnitX, vertCutSize, SceneAs<Level>(), slicerLength, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile, settings: sliceableEntityTypes));
                    }
                }
            }
        }
    }
}
