using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Entities
{
    public class PaperHitbox : Hitbox
    {
        private Paper Parent;

        public PaperHitbox(Paper paper, float width, float height, float x = 0, float y = 0) : base(width, height, x, y)
        {
            Parent = paper;
        }


        public override bool Collide(Hitbox hitbox)
        {
            List<Vector2> pointsToCheck = new List<Vector2>();

            for (float f1 = hitbox.AbsoluteLeft - this.Position.X; f1 < hitbox.AbsoluteRight - this.Position.X; f1 += 8)
            {
                for (float f2 = hitbox.AbsoluteTop - this.Position.Y; f2 < hitbox.AbsoluteBottom - this.Position.Y; f2 += 8)
                {
                    pointsToCheck.Add(new Vector2(f1, f2));
                }
            }

            foreach (Vector2 v in pointsToCheck)
            {
                int x = (int)v.X;
                int y = (int)v.Y;
                if (x >= 0 && y >= 0 && x < (int)Width && y < (int)Height)
                {
                    if (!Parent.skip[x / 8, y / 8])
                    {
                        return true;
                    }
                }
            }
            return base.Collide(hitbox);
        }

        public override bool Collide(Circle c)
        {
            for (float f1 = c.AbsoluteLeft - this.Position.X; f1 < c.AbsoluteRight - this.Position.X; f1 += 8)
            {
                for (float f2 = c.AbsoluteTop - this.Position.Y; f2 < c.AbsoluteBottom - this.Position.Y; f2 += 8)
                {

                    int x = (int)f1;
                    int y = (int)f2;
                    if (x >= 0 && y >= 0 && (int)x < (int)Width && (int)y < (int)Height)
                    {
                        if (!Parent.skip[(int)(x / 8), (int)(y / 8)])
                        {
                            if (c.Collide(new Rectangle((int)(f1 + Position.X), (int)(f2 + Position.Y), (int)(8), (int)(8)))) return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool CollidePaper(On.Monocle.Collider.orig_Collide_Collider orig, Collider self, Collider collider)
        {
            if (self is PaperHitbox || collider is PaperHitbox)
            {
                PaperHitbox ph = null;
                Collider other = null;
                if (self is PaperHitbox)
                {
                    ph = (self as PaperHitbox);
                    other = collider;
                }
                else if (collider is PaperHitbox)
                {
                    ph = (collider as PaperHitbox);
                    other = self;
                }

                if (other is PaperHitbox)
                {
                    return ph.Collide((Hitbox)other);
                } 
                else if (other is Hitbox)
                {
                    return ph.Collide((Hitbox)other);
                }
                else if (other is Circle)
                {
                    return ph.Collide((Circle)other);
                }
                else if (other is Grid)
                {
                    return ph.Collide((Grid)other);
                }
                else if (other is ColliderList)
                {
                    return (other as ColliderList).Collide(ph);
                }
                return false;
            }
            else if(collider is PaperHitbox)
            {
                PaperHitbox ph = (self as PaperHitbox);

                collider.Collide(self);
            }
            return orig.Invoke(self, collider);
        }

        public static void Load()
        {
            On.Monocle.Collider.Collide_Collider += CollidePaper;
        }

        public void Unload()
        {
            On.Monocle.Collider.Collide_Collider -= CollidePaper;

        }
    }
}
