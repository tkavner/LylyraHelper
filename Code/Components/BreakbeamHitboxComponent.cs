using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Other;

//wrapper type Collider for a ColliderList
public class BreakbeamHitboxComponent : Component
{
    public ColliderList clist { get; private set; }

    private Vector2 Offset;
    private Entity Parent;

    public int RawWidth { get; private set; }
    public string Orientation { get; private set; }

    public Hitbox[] hitboxes { get; }

    private ulong frame;
    public int scale = 8;
    private Level Level;
    private bool simpleHitbox;

    public BreakbeamHitboxComponent(int width, string orientation, Level level, Entity parent, ColliderList hitbox, Vector2 offset, int scale = 8, bool simple = false) : base(true, true)
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

            hitboxes[i] = GetOrientedHitbox(GetEdgeScreenLength(), scale, i * scale - width / 2);
            clist.Add(hitboxes[i]);
        }
        this.simpleHitbox = simple;
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
        var temp = Entity.Collider;
        Entity.Collider = clist;
        RecalcHitbox();
        Entity.Collider = temp;
        if (simpleHitbox)
        {
            float absoluteMin = int.MaxValue;
            foreach (Hitbox h in hitboxes)
            {
                if (Orientation == "up" || Orientation == "down")
                {
                    if (h.Height < absoluteMin) absoluteMin = h.Height;
                }
                else
                {

                    if (h.Width < absoluteMin) absoluteMin = h.Width;
                }
            }

            ResizeHitbox((Hitbox)Entity.Collider, absoluteMin, -hitboxes.Length * scale + RawWidth / 2);
        }
        else
        {
            Entity.Collider = clist;
        }

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
            ResizeHitbox(h, GetEdgeScreenLength(), i * scale - hitboxes.Length * scale / 2);
                
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
                    {
                        break;
                    }

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