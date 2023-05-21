﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Triggers
{

    [CustomEntity("LylyraHelper/AddSlicerTrigger")]
    public class AddSlicerTrigger : Trigger
    {
        private string direction;
        private bool sliceOnImpact;
        private bool oneUse;
        private bool fragile;
        private string[] entityTypes;
        private bool roomwide;

        public AddSlicerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            direction = data.Attr("direction", "All");
            sliceOnImpact = data.Bool("SliceOnImpact", false);
            oneUse = data.Bool("singleUse", false);
            fragile = data.Bool("fragileSlicer", false);
            entityTypes = data.Attr("entityTypes", "").Split(',');
        }

        public override void Update()
        {
            base.Update();
            foreach (Entity e in Scene.Entities)
            {
                if (e.Collider == null) continue;
                if (e.Get<Slicer>() != null) continue;
                bool tempCollide = this.Collidable;
                this.Collidable = true;
                bool collide = this.CollideCheck(e);
                this.Collidable = tempCollide;
                foreach (string typeName in entityTypes)
                {
                    if ((collide || roomwide) && e.GetType().FullName == typeName)
                    {
                        Collider cLeft = new Hitbox(e.Collider.Width + 5, e.Height + 0, -5, 0);
                        Collider cUp = new Hitbox(e.Collider.Width + 0, e.Height + 5, 0, -5);
                        Collider cDown = new Hitbox(e.Collider.Width + 0, e.Height + 5, 0, 0);
                        Collider cRight = new Hitbox(e.Collider.Width + 5, e.Height + 0, 0, 0);
                        if (direction == "All")
                        {
                            e.Add(new Slicer(Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            e.Add(new Slicer(-Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            e.Add(new Slicer(-Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            e.Add(new Slicer(Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile));

                            if (oneUse)
                            {
                                RemoveSelf();
                                break;
                            }
                        }
                        else if (direction == "Up")
                        {
                            e.Add(new Slicer(-Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cUp, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            if (oneUse)
                            {
                                RemoveSelf();
                                break;
                            }
                        }
                        else if (direction == "Down")
                        {
                            e.Add(new Slicer(Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cDown, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            if (oneUse)
                            {
                                RemoveSelf();
                                break;
                            }
                        }
                        else if (direction == "Right")
                        {
                            e.Add(new Slicer(Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cRight, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            if (oneUse)
                            {
                                RemoveSelf();
                                break;
                            }
                        }
                        else if (direction == "Left")
                        {
                            e.Add(new Slicer(-Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cLeft, sliceOnImpact: sliceOnImpact, fragile: fragile));
                            if (oneUse)
                            {
                                RemoveSelf();
                                break;
                            }
                        }
                    }
                }
            }
        }

    }
}