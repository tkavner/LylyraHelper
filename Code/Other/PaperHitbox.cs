using Celeste.Mod;
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

    //technically PaperHitbox is a type of Grid, however Grid colliding with Grid crashes the game, so we made our own
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

            for (float f1 = hitbox.AbsoluteLeft - Parent.Position.X; f1 < hitbox.AbsoluteRight - Parent.Position.X; f1 += 8)
            {
                for (float f2 = hitbox.AbsoluteTop - Parent.Position.Y; f2 < hitbox.AbsoluteBottom - Parent.Position.Y; f2 += 8)
                {
                    pointsToCheck.Add(new Vector2(f1, f2));
                }
            }
            pointsToCheck.Add(new Vector2(hitbox.AbsoluteLeft - Parent.Position.X, hitbox.AbsoluteBottom - Parent.Position.Y));
            pointsToCheck.Add(new Vector2(hitbox.AbsoluteRight - Parent.Position.X, hitbox.AbsoluteTop - Parent.Position.Y));
            pointsToCheck.Add(new Vector2(hitbox.AbsoluteRight - Parent.Position.X, hitbox.AbsoluteBottom - Parent.Position.Y));

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
            return false;
        }

        public override bool Collide(Circle circle)
        {
            List<Vector2> pointsToCheck = new List<Vector2>();

            for (float f1 = circle.AbsoluteLeft - Parent.Position.X; f1 < circle.AbsoluteRight - Parent.Position.X; f1 += 8)
            {
                for (float f2 = circle.AbsoluteTop - Parent.Position.Y; f2 < circle.AbsoluteBottom - Parent.Position.Y; f2 += 8)
                {
                    pointsToCheck.Add(new Vector2(f1, f2));
                }
            }
            pointsToCheck.Add(new Vector2(circle.AbsoluteLeft - Parent.Position.X, circle.AbsoluteBottom - Parent.Position.Y));
            pointsToCheck.Add(new Vector2(circle.AbsoluteRight - Parent.Position.X, circle.AbsoluteTop - Parent.Position.Y));
            pointsToCheck.Add(new Vector2(circle.AbsoluteRight - Parent.Position.X, circle.AbsoluteBottom - Parent.Position.Y));

            foreach (Vector2 v in pointsToCheck)
            {
                int x = (int)v.X;
                int y = (int)v.Y;
                if (x >= 0 && y >= 0 && x < (int)Width && y < (int)Height)
                {
                    if (!Parent.skip[x / 8, y / 8])
                    {
                        if (circle.Collide(new Rectangle((int)(x + Position.X), (int)(y + Position.Y), (int)(8), (int)(8)))) return true;
                    }
                }
            }
            return false;
        }

        private static bool CollidePaper(On.Monocle.Collider.orig_Collide_Collider orig, Collider self, Collider collider)
        {
            if ((self == null) || (collider == null)) return false;
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
                else if (other is Hitbox hitbox)
                {

                    return ph.Collide(hitbox);
                }
                else if (other is Circle circle)
                {
                    return ph.Collide(circle);
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
            return orig.Invoke(self, collider);
        }

        public override void Render(Camera camera, Color color)
        {
            base.Render(camera, color);
            for (int i = 0; i < Width / 8; i++)
            {
                for (int j = 0; j < Height / 8; j++)
                {
                    if (!Parent.skip[i, j])
                    {
                        Draw.HollowRect(Parent.Position + new Vector2(i * 8, j * 8), 8, 8, Color.Red);
                    }
                }
            }
        }

        public static void Load()
        {
            On.Monocle.Collider.Collide_Collider += CollidePaper;
        }

        public static void Unload()
        {
            On.Monocle.Collider.Collide_Collider -= CollidePaper;

        }
    }
}
