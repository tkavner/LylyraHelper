using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using FMOD;
using FMOD.Studio;
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
            private float countdown = 0.8F;
            private float timestamp;
            private int cutSize;
            private Sprite sprite;
            private BreakbeamHitboxComponent bbhc;
            private int LaserLength;
            private EventInstance beamAudioToken;
            private bool firePlayed;

            public Laser(Vector2 Position, Vector2 direction, Level level, int cutSize, LaserCutter parent, string strdirection, Vector2 offset) : base()
            {
                Collider = GetOrientedHitbox(2, strdirection, cutSize, 0);
                Add(bbhc = new BreakbeamHitboxComponent(cutSize, strdirection, level, parent, new ColliderList(), offset, simple:true));
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
                if (countdown <= 0.2 && !firePlayed)
                {
                    firePlayed = true;
                    Audio.Play("event:/LylyraHelper/laser_fire", Position, "fade", 0.8F);
                }
                if (countdown <= 0 && !Visible)
                {
                    Visible = true;
                    Collidable = true;
                    Parent.Collider = Parent.shortHitbox;
                    beamAudioToken = Audio.Play("event:/LylyraHelper/laser_beam");

                }
                int min = int.MaxValue;
                if (Direction.Y != 0)
                {
                    for (int x = 0; x < cutSize / 8; x++)
                    {
                        int num1 = x * 8 / bbhc.scale;
                        min = min > (int)bbhc.hitboxes[num1].Height ? (int)bbhc.hitboxes[num1].Height : min;

                    }
                }
                else if (Direction.X != 0)
                {
                    for (int y = 0; y < cutSize / 8; y++)
                    {
                        int num1 = y * 8 / bbhc.scale;
                        min = min > (int)bbhc.hitboxes[num1].Width ? (int)bbhc.hitboxes[num1].Width : min;
                    }
                }

                Slicer.directionalOffset = (min);
                LaserLength = min + 8;
                if (Direction.Y > 0 || Direction.X > 0) LaserLength = min;
                if (Visible) AddParticles();
            }

            private void AddParticles()
            {

                int num4 = Math.Abs((4 * ((int)LaserLength / 8)) - ((4 * ((int)LaserLength / 8)) + (LaserLength - (8 * ((int)LaserLength / 8)))));
                int laserRenderEndPoint = ((int)(8 * (((int)LaserLength / 2) / 8)));
                if (Direction.Y != 0)
                {
                    if (Direction.Y > 0)
                    {
                        //Parent.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, cutSize / 32 > 1 ? cutSize / 32 : 1, 
                            //new Vector2((int)Parent.BottomCenter.X , (int)(Parent.BottomCenter.Y + LaserLength)), new Vector2(cutSize / 2, 0), (float)Math.PI / 2);
                        Parent.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, cutSize / 32 > 1 ? cutSize / 32 : 1,
                            new Vector2((int)Parent.BottomCenter.X, (int)(Parent.BottomCenter.Y + LaserLength)), new Vector2(cutSize / 2, 0), Calc.HexToColor("afdbff"));
                    }
                    else if (Direction.Y < 0)
                    {
                        //Parent.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, cutSize / 32 > 1 ? cutSize / 32 : 1, 
                            //new Vector2((int)Parent.TopCenter.X, (int)(Parent.TopCenter.Y) - LaserLength), new Vector2(cutSize / 2, 0), (float)Math.PI * 3 / 2);
                        Parent.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, cutSize / 32 > 1 ? cutSize / 32 : 1,
                            new Vector2((int)Parent.TopCenter.X, (int)(Parent.TopCenter.Y - LaserLength)), new Vector2(cutSize / 2, 0), Calc.HexToColor("afdbff"));
                    }
                }
                else
                {
                    if (Direction.X > 0)
                    {
                        //Parent.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, cutSize / 32 > 1 ? cutSize / 32 : 1,
                            //new Vector2((int)Parent.CenterRight.X + LaserLength, (int)(Parent.CenterLeft.Y)), new Vector2(0, cutSize / 2));
                        Parent.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, cutSize / 32 > 1 ? cutSize / 32 : 1, 
                            new Vector2((int)Parent.CenterRight.X + LaserLength, (int)(Parent.CenterLeft.Y)), new Vector2(0, cutSize / 2), Calc.HexToColor("afdbff"), 0);
                    }
                    else if (Direction.X < 0)
                    {
                        //Parent.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, cutSize / 32 > 1 ? cutSize / 32 : 1, 
                            //new Vector2((int)Parent.CenterLeft.X - LaserLength, (int)(Parent.CenterLeft.Y)), new Vector2(0, cutSize / 2), (float)Math.PI);
                        Parent.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, cutSize / 32 > 1 ? cutSize / 32 : 1, 
                            new Vector2((int)Parent.CenterLeft.X - LaserLength, (int)(Parent.CenterLeft.Y)), new Vector2(0, cutSize / 2), Calc.HexToColor("afdbff"), (float) Math.PI);
                    }
                }
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (beamAudioToken != null)
                {
                    Audio.Stop(beamAudioToken);
                }
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                if (beamAudioToken != null)
                {
                    Audio.Stop(beamAudioToken);
                }

            }

            private void DrawMiddle()
            {
                int darkBlueLength = 4;
                int darkBlueOffset = 2;
                int whiteLength = 1;
                int whiteOffset = 6;
                int lightBlueLength = 1;
                int lightBlueOffset = 7;
                int num4 = Math.Abs((4 * ((int)LaserLength / 8)) - ((4 * ((int)LaserLength / 8)) + (LaserLength - (8 * ((int)LaserLength / 8)))));
                int laserRenderEndPoint = ((int)(8 * (((int)LaserLength / 2) / 8)));

                if (Direction.Y != 0)
                {
                    int num1, num2;
                    if (Direction.Y < 0)
                    {
                        num1 = (int)Parent.TopCenter.X - cutSize / 2;
                        num2 = (int)(Parent.TopCenter.Y) - laserRenderEndPoint - num4;
                        
                    } 
                    else
                    {
                        num1 = (int)Parent.BottomCenter.X - cutSize / 2;
                        num2 = (int)(Parent.BottomCenter.Y) + laserRenderEndPoint;
                    }
                    int num5 = num1 + cutSize;

                    Draw.Rect(new Rectangle(
                                num1 + darkBlueOffset,
                                num2,
                                darkBlueLength,
                                num4)
                            , Calc.HexToColor("afdbff"));
                    Draw.Rect(new Rectangle(
                            num1 + whiteOffset,
                            num2,
                            whiteLength,
                            num4)
                        , Color.White);
                    Draw.Rect(new Rectangle(
                            num1 + lightBlueOffset,
                            num2,
                            lightBlueLength,
                            num4)
                        , Calc.HexToColor("d0e8f4"));
                    Draw.Rect(new Rectangle(
                            num5 - darkBlueOffset - darkBlueLength,
                            num2,
                            darkBlueLength,
                            num4)
                        , Calc.HexToColor("afdbff"));
                    Draw.Rect(new Rectangle(
                            num5 - whiteOffset - whiteLength,
                            num2,
                            whiteLength,
                            num4)
                        , Color.White);
                    Draw.Rect(new Rectangle(
                            num5 - lightBlueOffset - lightBlueLength,
                            num2,
                            lightBlueLength,
                            num4)
                        , Calc.HexToColor("d0e8f4"));
                } 
                else
                {
                    int num1, num2;
                    if (Direction.X < 0)
                    {
                        num1 = (int)Parent.CenterLeft.X   - laserRenderEndPoint - num4;
                        num2 = (int)(Parent.CenterLeft.Y) - cutSize / 2;
                    }
                    else
                    {
                        laserRenderEndPoint = ((int)(8 * (((int)LaserLength / 2) / 8)));
                        num1 = (int)Parent.CenterRight.X + laserRenderEndPoint;
                        num2 = (int)(Parent.CenterRight.Y) - cutSize / 2;
                    }
                    int num5 = num2 + cutSize;

                    Draw.Rect(new Rectangle(
                            num1,
                            num2 + darkBlueOffset,
                            num4,
                            darkBlueLength),
                            Calc.HexToColor("afdbff"));
                    Draw.Rect(new Rectangle(
                            num1,
                            num2 + whiteOffset,
                            num4,
                            whiteLength),
                            Color.White);
                    Draw.Rect(new Rectangle(
                            num1,
                            num2 + lightBlueOffset,
                            num4,
                            lightBlueLength),
                            Calc.HexToColor("d0e8f4"));
                    Draw.Rect(new Rectangle(
                            num1,
                            num5 - darkBlueOffset - darkBlueLength,
                            num4,
                            darkBlueLength),
                            Calc.HexToColor("afdbff"));
                    Draw.Rect(new Rectangle(
                            num1,
                            num5 - whiteOffset - whiteLength,
                            num4,
                            whiteLength),
                            Color.White);
                    Draw.Rect(new Rectangle(
                            num1,
                            num5 - lightBlueOffset - lightBlueLength,
                            num4,
                            lightBlueLength),
                            Calc.HexToColor("d0e8f4"));
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
                    if (Direction.Y < 0) //up
                    {
                        Draw.Rect(new Rectangle((int)Parent.TopCenter.X - maxRectangleLength / 2, (int)(Parent.TopCenter.Y - 4), maxRectangleLength, 12), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.TopCenter.X - maxRectangleLength / 2, (int)(Parent.TopCenter.Y - 4), maxRectangleLength, 12), Calc.HexToColor("d0e8f4"));
                        DrawMiddle();
                        for (int x = 0; x < cutSize / 8; x++)
                        {
                            if ((x > 0 && x < cutSize / 8 - 1))
                                Draw.Rect(
                                    new Rectangle(
                                        (int)Parent.TopCenter.X + x * 8 - cutSize / 2, 
                                        (int)(Parent.TopCenter.Y) - LaserLength, 
                                        8, 
                                        LaserLength - 8), 
                                    Color.White);
                            for (int y = 0; y < ((int)LaserLength / 8); y++)
                            {

                                if (x > 0 && x < cutSize / 8 - 1 && y != 0) continue;
                                Vector2 coords = GetTileCoords(x, y, cutSize / 8, ((int)LaserLength) / 8);
                                if (coords == new Vector2(-1)) continue;
                                if (y < ((int)LaserLength / 8)/2)
                                    sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2(x * 8 - cutSize / 2, -(y + 1) * 8),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                else
                                {
                                    sprite.DrawSubrect(Parent.TopCenter - Position + new Vector2(x * 8 - cutSize / 2, -(y + 1) * 8 - (LaserLength - (8 * ((int)LaserLength / 8)))),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                }
                            }
                        }

                    }
                    else if (Direction.Y > 0) //down
                    {
                        Draw.Rect(new Rectangle((int)Parent.BottomCenter.X - maxRectangleLength / 2, (int)(Parent.BottomCenter.Y - 8), maxRectangleLength, 12), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.BottomCenter.X - maxRectangleLength / 2, (int)(Parent.BottomCenter.Y - 8), maxRectangleLength, 12), Calc.HexToColor("d0e8f4"));

                        DrawMiddle();
                        for (int x = 0; x < cutSize / 8; x++)
                        {
                            if ((x > 0 && x < cutSize / 8 - 1))
                                Draw.Rect(new Rectangle((int)Parent.BottomCenter.X + x * 8 - cutSize / 2, (int)(Parent.BottomCenter.Y), 8, LaserLength), Color.White);
                            for (int y = 0; y < ((int)LaserLength / 8);  y++)
                            {
                                Vector2 coords = GetTileCoords(x, y, cutSize / 8, ((int)LaserLength / 8));
                                if (coords == new Vector2(-1)) continue;
                                if (y < ((int)LaserLength / 8) / 2)
                                    sprite.DrawSubrect(Parent.BottomCenter - Position + new Vector2(x * 8 - cutSize / 2, y * 8),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                else
                                    sprite.DrawSubrect(Parent.BottomCenter - Position + new Vector2(x * 8 - cutSize / 2, y * 8 + (LaserLength - (8 * ((int)LaserLength / 8)))),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                            }
                        }
                    }
                    else if (Direction.X > 0) //right
                    {
                        Draw.Rect(new Rectangle((int)Parent.CenterRight.X - 8, (int)(Parent.CenterRight.Y - maxRectangleLength / 2), 16, maxRectangleLength), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.CenterRight.X - 8, (int)(Parent.CenterRight.Y - maxRectangleLength / 2), 16, maxRectangleLength), Calc.HexToColor("d0e8f4"));

                        DrawMiddle();
                        for (int y = 0; y < cutSize / 8; y++)
                        {
                            if ((y > 0 && y < cutSize / 8 - 1))
                                Draw.Rect(new Rectangle((int)Parent.CenterRight.X + 8, (int)(Parent.CenterRight.Y) + y * 8 - cutSize / 2, LaserLength - 8, 8), Color.White);
                            for (int x = 0; x < ((int)LaserLength / 8); x++)
                            {
                                if (y > 0 && y < cutSize / 8 - 1 && x != 0) continue;
                                Vector2 coords = GetTileCoords(x, y, ((int)LaserLength / 8), cutSize / 8);
                                if (coords == new Vector2(-1)) continue;
                                if (x < ((int)LaserLength / 8) / 2)
                                    sprite.DrawSubrect(Parent.CenterRight - Position + new Vector2(x * 8, (y) * 8 - cutSize / 2),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                else
                                    sprite.DrawSubrect(Parent.CenterRight - Position + new Vector2(x * 8 + (LaserLength - (8 * ((int)LaserLength / 8))), (y) * 8 - cutSize / 2),
                                    new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                            }
                        }
                    }
                    else if (Direction.X < 0)//left
                    {
                        Draw.Rect(new Rectangle((int)Parent.CenterLeft.X - 8, (int)(Parent.CenterLeft.Y - maxRectangleLength / 2), 16, maxRectangleLength), Color.White);
                        Draw.HollowRect(new Rectangle((int)Parent.CenterLeft.X - 8, (int)(Parent.CenterLeft.Y - maxRectangleLength / 2), 16, maxRectangleLength), Calc.HexToColor("d0e8f4"));
                        DrawMiddle();
                        for (int y = 0; y < cutSize / 8; y++)
                        {
                            if ((y > 0 && y < cutSize / 8 - 1))
                                Draw.Rect(new Rectangle((int)Parent.CenterLeft.X - LaserLength, (int)(Parent.CenterLeft.Y) + y * 8 - cutSize / 2, LaserLength - 8, 8), Color.White);
                            for (int x = 0; x < ((int)LaserLength / 8); x++)
                            {
                                if (y > 0 && y < cutSize / 8 - 1 && x != 0) continue;
                                Vector2 coords = GetTileCoords(x, y, ((int)LaserLength / 8), cutSize / 8);
                                if (coords == new Vector2(-1)) continue;
                                
                                if (x < ((int)LaserLength / 8) / 2)
                                    sprite.DrawSubrect(Parent.CenterLeft - Position + new Vector2(-(x + 1) * 8, (y) * 8 - cutSize / 2),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                                else
                                    sprite.DrawSubrect(Parent.CenterLeft - Position + new Vector2(-(x + 1) * 8 - (LaserLength - (8 * ((int)LaserLength / 8))), (y) * 8 - cutSize / 2),
                                        new Rectangle((int)coords.X * 8, (int)coords.Y * 8, 8, 8));
                            }
                        }
                    }
                }

                sprite.Visible = false;
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

            private Vector2 GetTileCoords(int i, int j, int iMax, int jMax)
            {
                if (Direction.Y < 0)
                {
                    if (i == 0 && j == 0) return new Vector2(0, 2);
                    else if (i == 0 && j == jMax - 1) return new Vector2(0, 0);
                    else if (i == 0) return new Vector2(0, 1);


                    else if (i == iMax - 1 && j == 0) return new Vector2(3, 2);
                    else if (i == iMax - 1 && j == jMax - 1) return new Vector2(3, 0);
                    else if (i == iMax - 1) return new Vector2(3, 1);

                    else if (i == 1 && j == 0) return new Vector2(1, 2);
                    else if (i == 1 && j == jMax - 1) return new Vector2(1, 0);
                    else if (i == 1) return new Vector2(1, 1);

                    else if (i == iMax - 2 && j == 0) return new Vector2(2, 2);
                    else if (i == iMax - 2 && j == jMax - 1) return new Vector2(2, 0);
                    else if (i == iMax - 2) return new Vector2(2, 1);


                    else if (j == 0) return new Vector2(5, 2);


                    return new Vector2(5, 1);
                }
                else if (Direction.Y > 0)
                {
                    if (i == 0 && j == 0) return new Vector2(0, 0);
                    else if (i == 0 && j == jMax - 1) return new Vector2(0, 2);
                    else if (i == 0) return new Vector2(0, 1);


                    else if (i == iMax - 1 && j == 0) return new Vector2(3, 0);
                    else if (i == iMax - 1 && j == jMax - 1) return new Vector2(3, 2);
                    else if (i == iMax - 1) return new Vector2(3, 1);

                    else if (i == 1 && j == 0) return new Vector2(1, 0);
                    else if (i == 1 && j == jMax - 1) return new Vector2(1, 2);
                    else if (i == 1) return new Vector2(1, 1);

                    else if (i == iMax - 2 && j == 0) return new Vector2(2, 0);
                    else if (i == iMax - 2 && j == jMax - 1) return new Vector2(2, 2);
                    else if (i == iMax - 2) return new Vector2(2, 1);




                    else if (j == 0) return new Vector2(5, 0);

                    return new Vector2(-1);
                }
                else if (Direction.X > 0) //right
                {
                    if (j == 0 && i == 0) return new Vector2(0, 0);
                    else if (j == 0 && i == iMax - 1) return new Vector2(2, 0);
                    else if (j == 0) return new Vector2(1, 0);

                    else if (j == jMax - 1 && i == 0) return new Vector2(0, 3);
                    else if (j == jMax - 1 && i == iMax - 1) return new Vector2(2, 3);
                    else if (j == jMax - 1) return new Vector2(1, 3);

                    else if (j == 1 && i == 0) return new Vector2(0, 1);
                    else if (j == 1 && i == iMax - 1) return new Vector2(2, 1);
                    else if (j == 1) return new Vector2(1, 1);

                    else if (j == jMax - 2 && i == 0) return new Vector2(0, 2);
                    else if (j == jMax - 2 && i == iMax - 1) return new Vector2(2, 2);
                    else if (j == jMax - 2) return new Vector2(1, 2);





                    else if (i == 0) return new Vector2(0, 5);
                    return new Vector2(-1);
                }
                else if (Direction.X < 0) //left
                {
                    if (j == 0 && i == 0) return new Vector2(2, 0);
                    else if (j == 0 && i == iMax - 1) return new Vector2(0, 0);
                    else if (j == 0) return new Vector2(1, 0);

                    else if (j == jMax - 1 && i == 0) return new Vector2(2, 3);
                    else if (j == jMax - 1 && i == iMax - 1) return new Vector2(0, 3);
                    else if (j == jMax - 1) return new Vector2(1, 3);

                    else if (j == 1 && i == 0) return new Vector2(2, 1);
                    else if (j == 1 && i == iMax - 1) return new Vector2(0, 1);
                    else if (j == 1) return new Vector2(1, 1);

                    else if (j == jMax - 2 && i == 0) return new Vector2(2, 2);
                    else if (j == jMax - 2 && i == iMax - 1) return new Vector2(0, 2);
                    else if (j == jMax - 2) return new Vector2(1, 2);


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
        private float cooldownTime;
        private FiringMode mode;
        private string flagName;
        private float firingLength;
        private float firingTimer;
        internal Collider shortHitbox;
        private Hitbox fullHitboxMain;
        private Hitbox fullHitboxSecondary;
        private Hitbox shortHitboxMain;
        private Hitbox shortHitboxSecondary;
        internal Collider bigHitbox;
        private bool invert;

        public LaserCutter(EntityData data, Vector2 offset, FiringMode fm) :
            base(data.Position + offset, 32, 32, false)
        {
            direction = data.Attr("direction", "Up");
            cutSize = data.Int("cutSize", 64);
            breakbeamThickness = data.Int("breakbeamThickness", 32);
            firingLength = data.Float("firingLength", 1F);
            cooldownTime = data.Float("cooldown", 2F);
            invert = data.Bool("invert", false);
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
                        breakbeam = new Breakbeam(Position - new Vector2(0, 640 - 16), this, new Vector2(breakbeamThickness, 640)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "down":
                        breakbeam = new Breakbeam(Position - new Vector2(0, -24), this, new Vector2(breakbeamThickness, 640)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "left":
                        breakbeam = new Breakbeam(Position - new Vector2(640 - 8, -4), this, new Vector2(640, breakbeamThickness)); //TODO Change breakbeam sizing after finishing spriting
                        break;
                    case "right":
                        breakbeam = new Breakbeam(Position - new Vector2(0, -4), this, new Vector2(640, breakbeamThickness)); //TODO Change breakbeam sizing after finishing spriting
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
                laserCooldown = cooldownTime;
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
                    Audio.Play("event:/LylyraHelper/laser_power_down");
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
                if (laserCooldown <= 0 && sprite.CurrentAnimationID == "idle" && mode == FiringMode.Flag && (SceneAs<Level>().Session.GetFlag(flagName) ^ invert))
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
