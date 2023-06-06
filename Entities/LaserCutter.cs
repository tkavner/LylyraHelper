using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using LylyraHelper.Other;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [CustomEntity(new string[]
{
    "LylyraHelper/LaserCutterPulse = LoadPulse",
    "LylyraHelper/LaserCutterBreakbeam = LoadBreakbeam",
    "LylyraHelper/LaserCutterInFront = LoadInFront",
    "LylyraHelper/LaserCutterFlag = LoadFlag"
})]
    public class LaserCutter : Solid
    {


        public static Entity LoadPulse(Level level, LevelData levelData, Vector2 offset, EntityData data) => new LaserCutter(data, offset, FiringMode.Pulse);
        public static Entity LoadBreakbeam(Level level, LevelData levelData, Vector2 offset, EntityData data) => new LaserCutter(data, offset, FiringMode.Breakbeam);
        public static Entity LoadInFront(Level level, LevelData levelData, Vector2 offset, EntityData data) => new LaserCutter(data, offset, FiringMode.bbnls);
        public static Entity LoadFlag(Level level, LevelData levelData, Vector2 offset, EntityData data) => new LaserCutter(data, offset, FiringMode.Flag);

        public class Laser : Entity
        {
            private Slicer Slicer;

            private LaserCutter Parent;
            private Random rand;
            private Vector2 Direction;
            private int counter;
            private float countdown = 0.7F;
            private float timestamp;
            private int cutSize;
            private Sprite sprite;
            private BreakbeamHitboxComponent bbhc;
            private int LaserLength;

            public Laser(Vector2 Position, Vector2 direction, Level level, int cutSize, LaserCutter parent, string strdirection, Vector2 offset) : base()
            {
                Collider = new ColliderList();
                Add(bbhc = new BreakbeamHitboxComponent(cutSize, strdirection, level, parent, (ColliderList) Collider, offset));
                TopCenter = Position;
                Direction = direction;
                this.Position = Position;
                this.Parent = parent;
                rand = new Random();
                Visible = false;
                Collidable = false;
                this.cutSize = cutSize;
                Add(sprite = LylyraHelperModule.SpriteBank.Create("laser"));
                sprite.Play(strdirection);
                sprite.Visible = false;
                Add(new PlayerCollider(OnPlayer));
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(Slicer = new Slicer(Direction, cutSize, SceneAs<Level>(), 320, Direction, sliceOnImpact: true, slicingCollider: Collider));
            }

            private void OnPlayer(Player obj)
            {
                if (countdown <= 0) obj.Die(Direction);
            }

            public override void Update()
            {
                base.Update();
                float newTimestamp = timestamp + Engine.DeltaTime;
                if (timestamp != newTimestamp)
                {
                    countdown -= newTimestamp - timestamp;
                    counter++;
                }
                if (countdown <= 0 && !Visible)
                {
                    Visible = true;
                    Collidable = true;
                    Parent.Collider = Parent.shortHitbox;
                    int min = int.MaxValue;
                    if (Direction.Y < 0)
                    {
                        for (int x = 0; x < cutSize / 8; x++)
                        {
                            int num1 = x * 8 / bbhc.scale;
                            min = min > (int)bbhc.hitboxes[num1].Height / 8 ? (int)bbhc.hitboxes[num1].Height / 8: min;

                        }
                        Slicer.directionalOffset = (min) * 8;
                        LaserLength = min + 1;
                    }
                    else if(Direction.Y > 0)
                    {
                        for (int x = 0; x < cutSize / 8; x++)
                        {
                            int num1 = x * 8 / bbhc.scale;
                            min = min > (int)bbhc.hitboxes[num1].Height / 8 ? (int)bbhc.hitboxes[num1].Height / 8 : min;

                        }
                        Slicer.directionalOffset = (min) * 8;
                        LaserLength = min + 1;
                    }
                    else if (Direction.X > 0)
                    {
                        for (int y = 0; y < cutSize / 8; y++)
                        {
                            int num1 = y * 8 / bbhc.scale;
                            min = min > (int)bbhc.hitboxes[num1].Width / 8  ? (int)bbhc.hitboxes[num1].Width / 8: min;
                        }
                        Slicer.directionalOffset = (min) * 8;
                        LaserLength = min + 1;
                    }
                    else
                    {
                        for (int y = 0; y < cutSize / 8; y++)
                        {
                            int num1 = y * 8 / bbhc.scale;
                            min = min > (int)bbhc.hitboxes[num1].Width / 8 ? (int)bbhc.hitboxes[num1].Width / 8 : min;
                        }
                        Slicer.directionalOffset = (min) * 8;
                        LaserLength = min + 1;
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                sprite.Visible = true;
                if (countdown <= 0)
                {
                    int maxRectangleLength = (int)(cutSize / 3.2F);
                    if (maxRectangleLength > 16) maxRectangleLength = 16; 
                    int min = LaserLength;
                    if (Direction.Y < 0)
                    {
                        Draw.Rect(new Rectangle((int)Parent.TopCenter.X - maxRectangleLength / 2, (int)(Parent.TopCenter.Y - 4), maxRectangleLength, 12), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.TopCenter.X - maxRectangleLength / 2, (int)(Parent.TopCenter.Y - 4), maxRectangleLength, 12), Calc.HexToColor("d0e8f4"));
                        
                        
                        for (int x = 0; x < cutSize / 8; x++)
                        {

                            if ((x > 0 && x < cutSize / 8 - 1))
                            {
                                Draw.Rect(new Rectangle((int)Parent.TopCenter.X + x * 8 - cutSize / 2, (int)(Parent.TopCenter.Y) - min * 8, 8, min * 8 - 8), Color.White);
                            }
                            for (int y = 0; y < min; y++)
                            {

                                if (x > 0 && x < cutSize / 8 - 1 && y != 0) continue;
                                Vector2 coords = GetTileCoords(x, y, cutSize / 8, min);
                                sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2(x * 8 - cutSize / 2, -(y + 1) * 8),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                if (coords.Y != 4)
                                {
                                    sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2(x * 8 - cutSize / 2, -(y + 1) * 8),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else if (coords.X == 1)
                                {
                                    sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2((x + 1) * 8 - cutSize / 2, -(y + 1) * 8),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else
                                {
                                    sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2((x - 1) * 8 - cutSize / 2, -(y + 1) * 8),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                            }
                        }

                    }
                    else if (Direction.Y > 0)
                    {
                        Draw.Rect(new Rectangle((int)Parent.BottomCenter.X - maxRectangleLength / 2, (int)(Parent.BottomCenter.Y - 8), maxRectangleLength, 12), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.BottomCenter.X - maxRectangleLength / 2, (int)(Parent.BottomCenter.Y - 8), maxRectangleLength, 12), Calc.HexToColor("d0e8f4"));
                        
                        for (int x = 0; x < cutSize / 8; x++)
                        {
                            if ((x > 0 && x < cutSize / 8 - 1))
                            {
                                Draw.Rect(new Rectangle((int)Parent.BottomCenter.X + x * 8 - cutSize / 2, (int)(Parent.BottomCenter.Y) + 8, 8, min * 8 - 8), Color.White);
                            }
                            for (int y = 0; y < LaserLength;  y++)
                            {
                                Vector2 coords = GetTileCoords(x, y, cutSize / 8, LaserLength);
                                if (coords == new Vector2(-1)) continue;
                                sprite.DrawSubrect(Parent.BottomCenter - Position + new Vector2(x * 8 - cutSize / 2, y * 8),
                                new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                
                            }
                        }
                    }
                    else if (Direction.X > 0) //right
                    {
                        Draw.Rect(new Rectangle((int)Parent.CenterRight.X - 8, (int)(Parent.CenterRight.Y - maxRectangleLength / 2), 16, maxRectangleLength), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.CenterRight.X - 8, (int)(Parent.CenterRight.Y - maxRectangleLength / 2), 16, maxRectangleLength), Calc.HexToColor("d0e8f4"));
                        
                        for (int y = 0; y < cutSize / 8; y++)
                        {
                            
                            if ((y > 0 && y < cutSize / 8 - 1))
                            {
                                Draw.Rect(new Rectangle((int)Parent.CenterRight.X + 8, (int)(Parent.CenterRight.Y) + y * 8 - cutSize / 2, min * 8 - 8, 8), Color.White);
                            }
                            for (int x = 0; x < min; x++)
                            {

                                Vector2 coords = GetTileCoords(x, y, min, cutSize / 8);

                                if (coords == new Vector2(-1)) continue;
                                if (coords.Y != 4)
                                {
                                    sprite.DrawSubrect(Parent.CenterRight - Position + new Vector2(x * 8, (y) * 8 - cutSize / 2),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else if (coords.X == 1)
                                {
                                    sprite.DrawSubrect(Parent.CenterRight - Position + new Vector2(x * 8, (y + 1) * 8 - cutSize / 2),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else
                                {
                                    sprite.DrawSubrect(Parent.CenterRight - Position + new Vector2(x * 8, (y - 1) * 8 - cutSize / 2),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                            }
                        }
                    }
                    else //left
                    {
                        Draw.Rect(new Rectangle((int)Parent.CenterLeft.X - 8, (int)(Parent.CenterLeft.Y - maxRectangleLength / 2), 16, maxRectangleLength), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.CenterLeft.X - 8, (int)(Parent.CenterLeft.Y - maxRectangleLength / 2), 16, maxRectangleLength), Calc.HexToColor("d0e8f4"));
                        
                        for (int y = 0; y < cutSize / 8; y++)
                        {

                            if ((y > 0 && y < cutSize / 8 - 1))
                            {
                                Draw.Rect(new Rectangle((int)Parent.CenterLeft.X + 8 - min * 8 - 8, (int)(Parent.CenterLeft.Y) + y * 8 - cutSize / 2, min * 8 - 8, 8), Color.White);
                            }
                            for (int x = 0; x < min; x++)
                            {
                                if (y > 0 && y < cutSize / 8 - 1 && x != 0) continue;
                                Vector2 coords = GetTileCoords(x, y, min, cutSize / 8);
                                if (coords.Y != 4)
                                {
                                    sprite.DrawSubrect(Parent.CenterLeft - Position + new Vector2(-(x + 1) * 8, (y) * 8 - cutSize / 2),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else if (coords.X == 1)
                                {
                                    sprite.DrawSubrect(Parent.CenterLeft - Position + new Vector2(-(x + 1) * 8, (y + 1) * 8 - cutSize / 2),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                                else
                                {
                                    sprite.DrawSubrect(Parent.CenterLeft - Position + new Vector2(-(x + 1) * 8, (y - 1) * 8 - cutSize / 2),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                            }
                        }
                    }
                }

                sprite.Visible = false;
            }


            private Vector2 GetTileCoords(int i, int j, int iMax, int jMax)
            {
                if (Direction.Y < 0)
                {
                    if (i == 0 && j == 0) return new Vector2(0, 2);
                    else if (i == 0 && j == jMax - 1) return new Vector2(0, 0);
                    else if (i == 0) return new Vector2(0, 1);

                    else if (i == 1 && j == 0) return new Vector2(1, 2);
                    else if (i == 1 && j == jMax - 1) return new Vector2(1, 0);
                    else if (i == 1) return new Vector2(1, 1);

                    else if (i == iMax - 2 && j == 0) return new Vector2(2, 2);
                    else if (i == iMax - 2 && j == jMax - 1) return new Vector2(2, 0);
                    else if (i == iMax - 2) return new Vector2(2, 1);

                    else if (i == iMax - 1 && j == 0) return new Vector2(3, 2);
                    else if (i == iMax - 1 && j == jMax - 1) return new Vector2(3, 0);
                    else if (i == iMax - 1) return new Vector2(3, 1);

                    else if (j == 0) return new Vector2(5, 2);


                    return new Vector2(5, 1);
                }
                else if (Direction.Y > 0)
                {
                    if (i == 0 && j == 0) return new Vector2(0, 0);
                    else if (i == 0 && j == jMax - 1) return new Vector2(0, 2);
                    else if (i == 0) return new Vector2(0, 1);



                    else if (i == 1 && j == 0) return new Vector2(1, 0);
                    else if (i == 1 && j == jMax - 1) return new Vector2(1, 2);
                    else if (i == 1) return new Vector2(1, 1);

                    else if (i == iMax - 2 && j == 0) return new Vector2(2, 0);
                    else if (i == iMax - 2 && j == jMax - 1) return new Vector2(2, 2);
                    else if (i == iMax - 2) return new Vector2(2, 1);

                    else if (i == iMax - 1 && j == 0) return new Vector2(3, 0);
                    else if (i == iMax - 1 && j == jMax - 1) return new Vector2(3, 2);
                    else if (i == iMax - 1) return new Vector2(3, 1);



                    else if (j == 0) return new Vector2(5, 0);

                    return new Vector2(-1);
                }
                else if (Direction.X > 0) //right
                {
                    if (j == 0 && i == 0) return new Vector2(0, 0);
                    else if (j == 0 && i == iMax - 1) return new Vector2(2, 0);
                    else if (j == 0) return new Vector2(1, 0);


                    else if (j == 1 && i == 0) return new Vector2(0, 1);
                    else if (j == 1 && i == iMax - 1) return new Vector2(2, 1);
                    else if (j == 1) return new Vector2(1, 1);

                    else if (j == jMax - 2 && i == 0) return new Vector2(0, 2);
                    else if (j == jMax - 2 && i == iMax - 1) return new Vector2(2, 2);
                    else if (j == jMax - 2) return new Vector2(1, 2);



                    else if (j == jMax - 1 && i == 0) return new Vector2(0, 3);
                    else if (j == jMax - 1 && i == iMax - 1) return new Vector2(2, 3);
                    else if (j == jMax - 1) return new Vector2(1, 3);


                    else if (i == 0) return new Vector2(0, 5);
                    return new Vector2(-1);
                }
                else if (Direction.X < 0) //left
                {
                    if (j == 0 && i == 0) return new Vector2(2, 0);
                    else if (j == 0 && i == iMax - 1) return new Vector2(0, 0);
                    else if (j == 0) return new Vector2(1, 0);

                    else if (j == 1 && i == 0) return new Vector2(2, 1);
                    else if (j == 1 && i == iMax - 1) return new Vector2(0, 1);
                    else if (j == 1) return new Vector2(1, 1);

                    else if (j == jMax - 2 && i == 0) return new Vector2(2, 2);
                    else if (j == jMax - 2 && i == iMax - 1) return new Vector2(0, 2);
                    else if (j == jMax - 2) return new Vector2(1, 2);

                    else if (j == jMax - 1 && i == 0) return new Vector2(2, 3);
                    else if (j == jMax - 1 && i == iMax - 1) return new Vector2(0, 3);
                    else if (j == jMax - 1) return new Vector2(1, 3);

                    else if (i == 0) return new Vector2(2, 5);
                    return new Vector2(1, 5);
                }
                return Vector2.Zero;
            }
        }
        public enum FiringMode
        {
            bbnls, Breakbeam, Flag, Pulse
        }

        public class Breakbeam : Entity
        {
            private LaserCutter Parent;


            public Breakbeam(Vector2 Position, LaserCutter parent, Vector2 size) : base(Position)
            {
                Add(new PlayerCollider(OnPlayer, new Hitbox(size.X, size.Y)));
                Parent = parent;
            }

            public Breakbeam(Vector2 Position, LaserCutter parent, int width, string orientation, Vector2 Offset) : base(Position)
            {
                Collider = new ColliderList();
                Add(new BreakbeamHitboxComponent(width, orientation, parent.SceneAs<Level>(), parent, (ColliderList)Collider, Offset));
                Add(new PlayerCollider(OnPlayer, Collider));
                Parent = parent;
            }

            private Hitbox GetOrientedHitbox(int length, string Orientation, int width = 0, int offset = 0)
            {
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

            public void OnPlayer(Player player)
            {
                if (Parent.mode == FiringMode.Breakbeam || Parent.mode == FiringMode.bbnls) Parent.Fire();
            }

        }



        private string direction;
        private int cutSize;
        private Sprite sprite;
        private Laser laser;
        private Breakbeam breakbeam;
        private float laserCooldown;
        private float timestamp;
        private float cooldown_time = 2.0F;
        private FiringMode mode;
        private float pulseFrequency;
        private string flagName;
        private float firingLength;
        private float firingTimer;
        internal Collider shortHitbox;
        private Hitbox fullHitboxMain;
        private Hitbox fullHitboxSecondary;
        private Hitbox shortHitboxMain;
        private Hitbox shortHitboxSecondary;
        internal Collider bigHitbox;

        public LaserCutter(EntityData data, Vector2 offset, FiringMode fm) :
            base(data.Position + offset, 32, 32, false)
        {
            direction = data.Attr("direction", "Up");
            cutSize = data.Int("cutSize", 64);
            breakbeamThickness = data.Int("breakBeamThickness", 32);
            firingLength = data.Float("firingLength", 1F);
            mode = fm;
            Add(sprite = LylyraHelperModule.SpriteBank.Create("laserCutter" + direction));
            direction = direction.ToLower();
            sprite.Play("idle");

            if (direction == "up")
            {

                fullHitboxMain = new Hitbox(32, 24, 0, 8);
                fullHitboxSecondary = new Hitbox(18, 6, 7, 2);
                shortHitboxMain = new Hitbox(32, 18, 0, 14);
                shortHitboxSecondary = new Hitbox(18, 6, 7, 6 + 2);
                sprite.SetOrigin(4, 8);
            }
            else if (direction == "down")
            {

                fullHitboxMain = new Hitbox(32, 24, 0, 0);
                fullHitboxSecondary = new Hitbox(18, 6, 7, 24);
                shortHitboxMain = new Hitbox(32, 18, 0, 0);
                shortHitboxSecondary = new Hitbox(18, 6, 7, 18);

                sprite.SetOrigin(4, 0);
            }
            else if (direction == "left")
            {
                fullHitboxMain = new Hitbox(24, 32, 8, 0);
                fullHitboxSecondary = new Hitbox(6, 18, 2, 7);
                shortHitboxMain = new Hitbox(18, 32, 14, 0);
                shortHitboxSecondary = new Hitbox(6, 18, 2 + 6, 7);
                sprite.SetOrigin(8, 4);
            }
            else
            {
                fullHitboxMain = new Hitbox(24, 32, 0, 0);
                fullHitboxSecondary = new Hitbox(6, 18, 24, 7);
                shortHitboxMain = new Hitbox(18, 32, 0, 0);
                shortHitboxSecondary = new Hitbox(6, 18, 24 - 6, 7);
                sprite.SetOrigin(0, 4);
            }
            bigHitbox = new ColliderList(fullHitboxMain, fullHitboxSecondary);
            shortHitbox = new ColliderList(shortHitboxMain, shortHitboxSecondary);
            Collider = bigHitbox;
            Visible = true;
        }

        private static Hitbox lerpHitboxes(Hitbox h1, Hitbox h2, float lerp)
        {
            if (lerp == 0) return h1;
            if (lerp == 1) return h2;
            return new Hitbox(
                h2.Width * lerp + (1 - lerp) * h1.Width,
                h2.Height * lerp + (1 - lerp) * h1.Height,
                h2.Position.X * lerp + (1 - lerp) * h1.Position.X,
                h2.Position.Y * lerp + (1 - lerp) * h1.Position.Y);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (mode == FiringMode.bbnls)
            {
                switch (direction.ToLower())
                {
                    case "up":
                        breakbeam = new Breakbeam(Position - new Vector2(0, 640 - 16), this, new Vector2(32, 640)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "down":
                        breakbeam = new Breakbeam(Position - new Vector2(0, -24), this, new Vector2(32, 640)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "left":
                        breakbeam = new Breakbeam(Position - new Vector2(640 - 8, -4), this, new Vector2(640, 32)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "right":
                        breakbeam = new Breakbeam(Position - new Vector2(0, -4), this, new Vector2(640, 32)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                }
                scene.Add(breakbeam);
            }

            else if (mode == FiringMode.Breakbeam)
            {
                switch (direction.ToLower())
                {
                    case "up":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(Width / 2, 0)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "down":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(Width / 2, 0)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "left":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(0, Height / 2)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "right":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(0, Height / 2)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                }


                scene.Add(breakbeam);

            }
        }
        private void Fire()
        {
            if (laserCooldown <= 0 && sprite.CurrentAnimationID == "idle")
            {
                sprite.Play("fire", false);
                firingTimer = firingLength;
                laserCooldown = cooldown_time;
                switch (direction.ToLower())
                {
                    case "up":
                        laser = new Laser(Position, -Vector2.UnitY, SceneAs<Level>(), cutSize, this, direction, new Vector2(Width / 2, 0));
                        break;
                    case "down":
                        laser = new Laser(Position, Vector2.UnitY, SceneAs<Level>(), cutSize, this, direction, new Vector2(Width / 2, 0));
                        break;
                    case "right":
                        laser = new Laser(Position, Vector2.UnitX, SceneAs<Level>(), cutSize, this, direction, new Vector2(0, Height / 2));
                        break;
                    case "left":
                        laser = new Laser(Position, -Vector2.UnitX, SceneAs<Level>(), cutSize, this, direction, new Vector2(0, Height / 2));
                        break;
                }
                Scene.Add(laser);
            }
        }

        private float lerp = 0;
        private int breakbeamThickness;

        public override void Update()
        {
            base.Update();
            float newTimestamp = timestamp + Engine.DeltaTime;
            if (timestamp != newTimestamp)
            {
                laserCooldown -= Engine.DeltaTime;
                firingTimer -= Engine.DeltaTime;
                if (firingTimer <= 0 && (sprite.CurrentAnimationID == "fire" || sprite.CurrentAnimationID == "firecont"))
                {
                    sprite.Play("reset");
                    Scene.Remove(laser);
                    laser = null;
                    lerp = 0;
                }
                if (sprite.CurrentAnimationID == "reset" && Collider != bigHitbox)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    Vector2 oldPos = Position;
                    lerp += Engine.DeltaTime;
                    if (lerp > 1)
                    {
                        lerp = 1;
                        Collider = bigHitbox;
                        switch (direction)
                        {
                            case "up":
                                MoveVCollideSolids(-6, false);
                                break;
                            case "down":
                                MoveVCollideSolids(6, false);
                                break;
                        }

                    }

                    switch (direction)
                    {
                        case "right":
                            Collider = new ColliderList(lerpHitboxes(shortHitboxMain, fullHitboxMain, lerp), lerpHitboxes(shortHitboxSecondary, fullHitboxSecondary, lerp));
                            MoveHCollideSolids(Engine.DeltaTime * 12, false);
                            break;
                        case "left":
                            Collider = new ColliderList(lerpHitboxes(shortHitboxMain, fullHitboxMain, lerp), lerpHitboxes(shortHitboxSecondary, fullHitboxSecondary, lerp));
                            MoveHCollideSolids(-Engine.DeltaTime * 12, false);
                            break;
                    }
                    Position = oldPos;
                }
                else if (sprite.CurrentAnimationID == "idle")
                {
                    Collider = bigHitbox;
                }
                if (laserCooldown <= 0 && sprite.CurrentAnimationID == "idle" && mode == FiringMode.Pulse)
                {
                    Fire();
                }
                if (laserCooldown <= 0 && sprite.CurrentAnimationID == "idle" && mode == FiringMode.Flag && SceneAs<Level>().Session.GetFlag(flagName))
                {
                    Fire();
                }
            }
        }
        public override void Render()
        {
            if (laser != null) laser.Render();
            base.Render();
        }


    }
}
