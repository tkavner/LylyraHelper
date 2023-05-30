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
    [CustomEntity("LylyraHelper/LaserCutter")]
    public class LaserCutter : Solid
    {
        public class Laser : Entity
        {
            private Slicer Slicer;

            private class EnergyLine
            {
                public Vector2 Position;
                public Vector2 Size;
                public Vector2 Direction;
                public Color Color;
                public Vector2 BoundsX;
                public Vector2 BoundsY;

                public static EnergyLine GetEnergyLine(bool xOriented, Random rand, Vector2 beamSize, Vector2 Position, Vector2 boundsX, Vector2 boundsY)
                {

                    int siz = rand.Next((int)beamSize.Y / 5, (int)beamSize.Y * 4 / 5);
                    int pos = rand.Next(0, (int)(beamSize.Y - siz - 1));
                    return xOriented ? new EnergyLine //x
                    {
                        Position = Position + new Vector2(rand.Next(0, (int)(beamSize.X - 1)), rand.Next(0, (int)(beamSize.Y - siz))),
                        Size = new Vector2(rand.Next((int)beamSize.Y / 5, (int)beamSize.Y), rand.Next(1, 3)),
                        Direction = new Vector2(rand.Next(0, 2) * 2 - 1, 0),
                        Color = Color.Lerp(Color.White, Calc.HexToColor("cdf38c"), rand.NextFloat()),
                        BoundsX = boundsX,
                        BoundsY = boundsY
                    } : new EnergyLine //y
                    {
                        Position = Position + new Vector2(rand.Next(0, (int)(beamSize.X - 1)), pos),
                        Size = new Vector2(rand.Next(1, 3), rand.Next((int)beamSize.Y / 5, siz)),
                        Direction = (new Vector2(rand.Next(0, 2) * 2 - 1, rand.Next(0, 2) * 2 - 1)),
                        Color = Color.Lerp(Color.White, Calc.HexToColor("cdf38c"), rand.NextFloat()),
                        BoundsX = boundsX,
                        BoundsY = boundsY
                    };
                }

                internal void Update()
                {
                    if (Position.X < BoundsX.X ||
                        Position.X > BoundsX.Y
                        )
                    {
                        Direction.X *= -1;
                    }
                    if (Position.Y < BoundsY.X ||
                        Position.Y > BoundsY.Y)
                    {
                        Direction.Y *= -1;
                    }

                    Position += Direction;
                }

                internal void Render()
                {
                    Draw.Rect(Position, Size.X, Size.Y, Color);
                }
            }

            private LaserCutter Parent;
            private Random rand;
            private MTexture[,] beamTextures;
            private Vector2 Direction;
            private int counter;
            private float countdown = 0.7F;
            private float timestamp;
            private int cutSize;

            public Laser(Vector2 Position, int width, int height, Vector2 direction, Level level, int cutSize, LaserCutter parent) : base(Position)
            {
                Collider = new Hitbox(width, height);
                Direction = direction;
                this.Position = Position;
                this.Parent = parent;
                rand = new Random();
                beamTextures = new MTexture[2, 4];
                Visible = false;
                Collidable = false;
                this.cutSize = cutSize;
                string tex1 = direction.X == 0 ? "objects/LylyraHelper/laserCutter/laserFirstVertical" : "objects/LylyraHelper/laserCutter/laserFirstHorizontal";
                string tex2 = direction.X == 0 ? "objects/LylyraHelper/laserCutter/laserSecondVertical" : "objects/LylyraHelper/laserCutter/laserSecondHorizontal";
                if (direction.X == 0)
                {
                    beamTextures[0, 0] = GFX.Game[tex1].GetSubtexture(new Rectangle(0, 0, 16, 16));
                    beamTextures[0, 1] = GFX.Game[tex1].GetSubtexture(new Rectangle(0, 16, 16, 16));
                    beamTextures[0, 2] = GFX.Game[tex1].GetSubtexture(new Rectangle(0, 32, 16, 16));
                    beamTextures[0, 3] = GFX.Game[tex1].GetSubtexture(new Rectangle(0, 48, 16, 16));
                    beamTextures[1, 0] = GFX.Game[tex2].GetSubtexture(0, 0, 16, 16);
                    beamTextures[1, 1] = GFX.Game[tex2].GetSubtexture(0, 16, 16, 16);
                    beamTextures[1, 2] = GFX.Game[tex2].GetSubtexture(0, 32, 16, 16);
                    beamTextures[1, 3] = GFX.Game[tex2].GetSubtexture(0, 48, 16, 16);
                } 
                else
                {
                    beamTextures[0, 0] = GFX.Game[tex1].GetSubtexture(new Rectangle(0,  0, 16, 16));
                    beamTextures[0, 1] = GFX.Game[tex1].GetSubtexture(new Rectangle(16, 0, 16, 16));
                    beamTextures[0, 2] = GFX.Game[tex1].GetSubtexture(new Rectangle(32, 0, 16, 16));
                    beamTextures[0, 3] = GFX.Game[tex1].GetSubtexture(new Rectangle(48, 0, 16, 16));
                    beamTextures[1, 0] = GFX.Game[tex2].GetSubtexture(0, 0, 16, 16);
                    beamTextures[1, 1] = GFX.Game[tex2].GetSubtexture(16, 0, 16, 16);
                    beamTextures[1, 2] = GFX.Game[tex2].GetSubtexture(32, 0, 16, 16);
                    beamTextures[1, 3] = GFX.Game[tex2].GetSubtexture(48, 0, 16, 16);
                }
                
                Add(new PlayerCollider(OnPlayer));
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(Slicer = new Slicer(new Vector2(Math.Abs(Direction.X), Math.Abs(Direction.Y)), cutSize, SceneAs<Level>(), 320, sliceOnImpact: true));
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
                }
            }

            public override void Render()
            {
                base.Render();

                if (countdown <= 0)
                {
                    if (Direction.Y < 0)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            if (i == 0) beamTextures[counter / 20 % 2, 2].DrawCentered(Position + new Vector2(Width / 2, Height));
                            else beamTextures[(counter / 20) % 2, (i + counter / 20) % 2 + 1].DrawCentered(Position + new Vector2(Width / 2, Height + Parent.Height / 2) - new Vector2(0, i * 16));
                        }
                    }
                    else if(Direction.Y > 0)
                    {
                        for (int i = 0; i < 40; i++)
                        {   
                            if (i == 0) beamTextures[counter / 20 % 2, 2].DrawCentered(Position + new Vector2(Width / 2, Height));
                            else beamTextures[(counter / 20) % 2, (i + counter / 20) % 2 + 1].DrawCentered(Position + new Vector2(8, i * 16));
                        }
                    }
                    else if (Direction.X > 0) //right
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            if (i == 0) { }
                            else beamTextures[(counter / 20) % 2, (i + counter / 20) % 2 + 1].DrawCentered(Parent.Center - new Vector2(Parent.Width / 2, 0) + new Vector2(i * 16, 0));
                        }
                    }
                    else //left
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            if (i == 0) { }
                            else beamTextures[(counter / 20) % 2, (i + counter / 20) % 2 + 1].DrawCentered(Parent.Center + new Vector2(Parent.Width / 2, 0) - new Vector2(i * 16, 0));
                        }
                    }
                }
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
                Add(new BreakbeamHitboxComponent(width, orientation, parent.SceneAs<Level>(), parent, (ColliderList) Collider, Offset));
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
                Parent.Fire();
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

        public LaserCutter(EntityData data, Vector2 offset) :
            base(data.Position + offset, 32, 32, false)
        {
            direction = data.Attr("direction", "Up").ToLower();
            cutSize = data.Int("cutSize", 16);
            string strmode = data.Attr("mode", "breakbeamlos");
            breakbeamThickness = data.Int("breakBeamThickness", 32);
            bool flag0 = (strmode.ToLower() == "breakbeamlos");
            Logger.Log(LogLevel.Error, "LylyraHelper", direction);
            Logger.Log(LogLevel.Error, "LylyraHelper", strmode);
            Logger.Log(LogLevel.Error, "LylyraHelper", "" + flag0);
            firingLength = data.Float("firingLength", 1F);
            if (strmode.ToLower() == "breakbeamlos")
            {
                mode = FiringMode.Breakbeam;
            }
            else if (strmode.ToLower() == "breakbeam")
            {
                mode = FiringMode.bbnls;
            }
            else if (strmode.ToLower() == "flag")
            {
                mode = FiringMode.Flag;
                flagName = data.Attr("flag", "laser_cutter_activate");
                cooldown_time = data.Float("frequency", 2.0F);
            }
            else if (strmode.ToLower() == "pulse")
            {
                mode = FiringMode.Pulse;
                cooldown_time = data.Float("frequency", 2.0F);
            } 
            else
            {
                throw new Exception("Invalid Laser Cutter Firing Mode: " + strmode);
            }
            Add(sprite = LylyraHelperModule.SpriteBank.Create("laserCutter"));
            sprite.Play("idle");

            if (direction == "up")
            {

                fullHitboxMain = new Hitbox(32, 24, 0, 12);
                fullHitboxSecondary = new Hitbox(18, 6, 7, 6);
                shortHitboxMain = new Hitbox(32, 18, 0, 18);
                shortHitboxSecondary = new Hitbox(18, 6, 7, 6 + 6);
                sprite.Rotation = 0;
                sprite.SetOrigin(4, 4);
            }
            else if (direction == "down")
            {

                fullHitboxMain = new Hitbox(32, 24, 0, 4);
                fullHitboxSecondary = new Hitbox(18, 6, 7, 28);
                shortHitboxMain = new Hitbox(32, 18, 0, 4);
                shortHitboxSecondary = new Hitbox(18, 6, 7, 28 - 6);
                sprite.Rotation = (float)Math.PI;

                sprite.SetOrigin(36, 44);
            }
            else if (direction == "left")
            {
                fullHitboxMain = new Hitbox(24, 32, 8, 4);
                fullHitboxSecondary = new Hitbox(6, 18, 2, 11);
                shortHitboxMain = new Hitbox(18, 32, 14, 4);
                shortHitboxSecondary = new Hitbox(6, 18, 2 + 6, 11);
                sprite.Rotation = (float)Math.PI * 3F / 2F;
                sprite.SetOrigin(40, 8);
            }
            else
            {
                fullHitboxMain = new Hitbox(24, 32, 0, 4);
                fullHitboxSecondary = new Hitbox(6, 18, 24, 11);
                shortHitboxMain = new Hitbox(18, 32, 0, 4);
                shortHitboxSecondary = new Hitbox(6, 18, 24 - 6, 11);
                sprite.SetOrigin(0, 40);
                sprite.Rotation = (float)Math.PI * 1F / 2F;
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
                h2.Width * lerp  + (1 - lerp) * h1.Width,
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
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(Width / 2, 4)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "down":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(Width / 2, 4)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "left":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(0, 4)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "right":
                        breakbeam = new Breakbeam(Position, this, breakbeamThickness, direction, new Vector2(0, 4)); //TODO Change breakbeam sizing after finishing spriting
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
                        laser = new Laser(BottomLeft + new Vector2(8, -648), 16, 640, -Vector2.UnitY, SceneAs<Level>(), cutSize, this);
                        break;
                    case "down":
                        laser = new Laser(TopLeft + new Vector2(8, 0), 16, 640, Vector2.UnitY, SceneAs<Level>(), cutSize, this);
                        break;
                    case "right":
                        laser = new Laser(TopRight + new Vector2(0, 8), 640, 16, Vector2.UnitX, SceneAs<Level>(), cutSize, this);
                        break;
                    case "left":
                        laser = new Laser(TopRight + new Vector2(-648, 8), 640, 16, -Vector2.UnitX, SceneAs<Level>(), cutSize, this);
                        break;
                }
                Scene.Add(laser);
            }
            Logger.Log(LogLevel.Error, "LylyraHelper", mode.ToString());
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
                } else if (sprite.CurrentAnimationID == "idle")
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
