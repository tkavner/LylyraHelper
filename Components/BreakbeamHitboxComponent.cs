using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Other
{
    //wrapper type Collider for a ColliderList
    public class BreakbeamHitboxComponent : Component
    {
        public ColliderList clist { get; private set; }

        private Vector2 Offset;
        private Entity Parent;

        public int RawWidth { get; private set; }
        public string Orientation { get; private set; }

        private Hitbox[] hitboxes;

        private ulong frame;
        private int scale = 4;
        private Level Level;
        public BreakbeamHitboxComponent(int width, string orientation, Level level, Entity parent, ColliderList hitbox, Vector2 offset) : base(true, true)
        {
            RawWidth = width;
            Orientation = orientation;
            hitboxes = new Hitbox[width / scale];
            Level = level;
            Parent = parent;
            this.clist = hitbox;
            this.Offset = offset;
            for (int i = 0; i < hitboxes.Length; i++)
            {
                Logger.Log(LogLevel.Error, "Lylyra", string.Format("{0}", hitboxes.Length));

                hitboxes[i] = GetOrientedHitbox(GetEdgeScreenLength(), scale, i * scale - width / 2);
                clist.Add(hitboxes[i]);
            }
        }

        private int GetEdgeScreenLength()
        {
            if (Orientation == "up")
            {
                return (int)Math.Abs(Parent.Position.Y - Level.Bounds.Top);
            }
            else if (Orientation == "down") 
            { 
                return (int)Math.Abs(Parent.Position.Y - Level.Bounds.Bottom);
            }
            else if (Orientation == "right")
            {
                return (int)Math.Abs(Parent.Position.X - Level.Bounds.Right);
            }
            else
            {
                return (int)Math.Abs(Parent.Position.X - Level.Bounds.Left);
            }
        }

        private Hitbox GetOrientedHitbox(int length, int width = 0, int offset = 0)
        {
            if (width == 0) width = RawWidth;
            if (Orientation == "up")
            {
                return new Hitbox(width, length, offset, -length);
            } 
            else if (Orientation == "down")
            {
                return new Hitbox(width, length, offset, 0);
            }
            else if (Orientation == "right")
            {
                return new Hitbox(length, width, -length, offset);
            }
            else if (Orientation == "left")
            {
                return new Hitbox(length, width, 0, offset);
            }
            else
            {
                throw new Exception("Invalid Breakbeam Orientation: " + Orientation);
            }
        }

        public override void Update()
        {
            base.Update();
            RecalcHitbox();
        }

        public ColliderList GetHitbox()
        {
            //check if we have already calculated breakbeam this frame
            if (Engine.FrameCounter == frame)
            {
                return clist;
            }
            frame = Engine.FrameCounter;
            RecalcHitbox();
            return clist;
        }

        private void ResizeHitbox(Hitbox hitbox, float size, float offset)
        {
            switch (Orientation)
            {
                case "up":
                    hitbox.Height = size;
                    hitbox.Left = offset + Offset.X;
                    hitbox.Top = -size + Offset.Y;
                    break;

                case "down":
                    hitbox.Height = size;
                    hitbox.Left = offset + Offset.X;
                    hitbox.Top = Parent.Height + Offset.Y;
                    break;

                case "left":
                    hitbox.Width = size;
                    hitbox.Top = offset + Offset.Y;
                    hitbox.Left = -size + Offset.X;
                    break;

                case "right":
                    hitbox.Width = size;
                    hitbox.Top = offset + Offset.Y;
                    hitbox.Left = Parent.Width + Offset.X;
                    break;
            }
        }

        private void RecalcHitbox()
        {

            for (int i = 0; i < RawWidth / scale; i++)
            {
                //update each hitbox
                Hitbox h = hitboxes[i];
                ResizeHitbox(h, GetEdgeScreenLength(), i * scale - RawWidth / scale * 2);

                if (Level.CollideCheck<Solid>(new Rectangle((int) h.AbsoluteLeft, (int) h.AbsoluteTop, (int) h.Width, (int) h.Height)))
                {
                    int low = 0;
                    int high = GetEdgeScreenLength();
                    int cycles = 10 + (int) Math.Log(GetEdgeScreenLength(), 2);
                    while (cycles-- > 0)
                    {

                        int pivot = (int)(low + (high - low) / 2f);
                        ResizeHitbox(h, pivot, i * scale - hitboxes.Length * scale / 2); 

                        if (pivot == low)
                            break;
                        if (Level.CollideCheck<Solid>(new Rectangle((int)h.AbsoluteLeft, (int)h.AbsoluteTop, (int)h.Width, (int)h.Height)))
                        {
                            high = pivot;
                        }
                        else
                        {
                            low = pivot;
                        }
                    }
                }

            }
        }


    }
}
