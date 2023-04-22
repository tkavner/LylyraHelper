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
    [CustomEntity("LylyraHelper/AddSlicerOnStartTrigger")]
    public class AddSlicerOnStartTrigger : Trigger
    {
        private Vector2 direction;
        private bool sliceOnImpact;

        public AddSlicerOnStartTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            string strdirection = data.Attr("Direction", "all");
            if (strdirection == "up")
            {
                direction = -Vector2.UnitY;
            } else if (strdirection == "down")
            {
                direction = Vector2.UnitY;
            }
            else if (strdirection == "right")
            {
                direction = Vector2.UnitX;
            }
            else if (strdirection == "left")
            {
                direction = -Vector2.UnitX;
            }
            sliceOnImpact = data.Bool("SliceOnImpact", false);
        }

        public override void Update()
        {
            base.Update();
            foreach (Entity e in Scene.Tracker.GetEntities<Solid>())
            {
                if (e.Collider == null) continue;

                Collider cLeft = new Hitbox(e.Collider.Width + 5, e.Height + 0, -5, 0);
                Collider cUp = new Hitbox(e.Collider.Width + 0, e.Height + 5, 0, -5);
                Collider cDown = new Hitbox(e.Collider.Width + 0, e.Height + 5, 0, 0);
                Collider cRight = new Hitbox(e.Collider.Width + 5, e.Height + 0, 0, 0);
                if (e is CrushBlock)
                {
                    e.Add(new Slicer(Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cRight, sliceOnImpact: sliceOnImpact));
                    e.Add(new Slicer(-Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cUp, sliceOnImpact: sliceOnImpact));
                    e.Add(new Slicer(-Vector2.UnitX, (int)e.Height + 8, SceneAs<Level>(), 5, cLeft, sliceOnImpact: sliceOnImpact));
                    e.Add(new Slicer(Vector2.UnitY, (int)e.Width + 8, SceneAs<Level>(), 5, cDown, sliceOnImpact: sliceOnImpact));

                }
            }
            RemoveSelf();
        }
    }
}
