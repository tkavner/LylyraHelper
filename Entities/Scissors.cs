using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Intefaces;
using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked(true)]
    public class Scissors : Entity
    {
        private List<CuttablePaper> Cutting = new List<CuttablePaper>();
        private Vector2 CutDirection;

        private float timeElapsed;

        private Sprite sprite;
        private bool Moving = true;
        private bool playedAudio;
        private string directionPath;
        private List<DreamBlock> DreamCutting = new List<DreamBlock>();
        private List<FallingBlock> FallCutting = new List<FallingBlock>();
        private List<CrushBlock> KevinCutting = new List<CrushBlock>();

        private List<CrushBlock> KevinCuttingActivationList = new List<CrushBlock>();

        private bool fragile;

        private Level level;
        private Shaker shaker;

        private int cutSize = 32;
        private bool breaking;
        private float breakTimer;

        public Scissors(Vector2[] nodes, Vector2 direction, bool fragile = false) : this(nodes[0], direction, fragile)
        {

        }

        public Scissors(Vector2 Position, Vector2 direction, bool fragile = false) : base(Position)
        {
            this.CutDirection = direction;
            if (direction.X > 0)
            {
                directionPath = "right";
            }
            else if (direction.X < 0)
            {
                directionPath = "left";
            }
            else if (direction.Y > 0)
            {
                directionPath = "down";
            }
            else
            {
                directionPath = "up";
            }
            this.Position = Position;

            sprite = new Sprite(GFX.Game, "objects/LylyraHelper/scissors/");
            sprite.AddLoop("spawn", "cut" + directionPath, 0.1F, new int[] { 0 });
            sprite.AddLoop("idle", "cut" + directionPath, 0.1F);

            sprite.AddLoop("break", "break" + directionPath, 0.1F);
            sprite.Play("spawn");
            Add(sprite);
            sprite.CenterOrigin();
            sprite.Visible = true;
            base.Collider = new ColliderList(new Circle(10F), new Hitbox(24, 4f, -12f, -2f));
            Add(new PlayerCollider(OnPlayer));
            this.fragile = fragile;
            Add(shaker = new Shaker());
            Add(new Coroutine(Break()));
        }

        private IEnumerator Break()
        {
            while (!breaking) yield return null;
            Moving = false;
            sprite.Play("break");
            Collidable = false;

            //TODO add code to early cut?
            yield return 0.75F;
            Scene.Remove(this);
            yield return null;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        private void OnPlayer(Player player)
        {
            if (timeElapsed > 1)
            {
                player.Die((player.Position - Position).SafeNormalize());
                Moving = false;
                sprite.Stop();
            }
        }

        public override void Update()
        {
            base.Update();
            float oldElapsed = timeElapsed;
            timeElapsed += Engine.DeltaTime;

            if (timeElapsed != oldElapsed) //check for frame advacement
            {
                if (Moving)
                {
                    if (oldElapsed == 0)
                    {
                        Level level = SceneAs<Level>();
                        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
                        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
                        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
                        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    }
                    if (timeElapsed > 1)
                    {
                        this.Position += (CutDirection).SafeNormalize() * 3;
                        sprite.CenterOrigin();
                        sprite.Visible = true;
                        sprite.Play("idle");
                        if (!playedAudio)
                        {
                            playedAudio = true;
                        }
                    }
                    CheckCollisions();

                    CutPaper();
                    CutDreamBlocks();
                    CutKevins();
                    CutFallBlocks();
                }


            }
        }

        private void CheckCollisions()
        {
            //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for sppeed)
            foreach (CuttablePaper d in base.Scene.Tracker.GetEntities<DashPaper>())
            {
                if (!Cutting.Contains(d)) if (this.CollideCheck(d)) Cutting.Add(d);
            }

            foreach (CuttablePaper d in base.Scene.Tracker.GetEntities<DeathNote>())
            {
                if (!Cutting.Contains(d)) if (this.CollideCheck(d)) Cutting.Add(d);
            }

            foreach (DreamBlock d in base.Scene.Tracker.GetEntities<DreamBlock>())
            {
                int x1 = (int)d.Position.X;
                int x2 = (int)(Position.X);

                int y1 = (int)d.Position.Y;
                int y2 = (int)(Position.Y);
                if (!DreamCutting.Contains(d) && this.CollideCheck(d))
                {
                    if (!(x1 == x2 ||
                        y1 == y2 ||
                        x1 + d.Width <= x2 ||
                        y1 + d.Height <= y2))
                    {
                        DreamCutting.Add(d);
                    }
                }
            }
            foreach (Solid d in base.Scene.Tracker.GetEntities<Solid>())
            {
                if (d.GetType() == typeof(CrushBlock))
                {
                    if (!KevinCutting.Contains(d) && this.CollideCheck(d))
                    {
                        int x1 = (int)d.Position.X;
                        int x2 = (int)(Position.X);

                        int y1 = (int)d.Position.Y;
                        int y2 = (int)(Position.Y);
                        if (!(x1 == x2 ||
                            y1 == y2 ||
                            x1 + d.Width <= x2 ||
                            y1 + d.Height <= y2))
                        {
                            KevinCutting.Add((CrushBlock)d);
                        }
                    }
                }
                else if (d.GetType() == typeof(FallingBlock))
                {
                    if (!FallCutting.Contains(d) && this.CollideCheck(d))
                    {
                        int x1 = (int)d.Position.X;
                        int x2 = (int)(Position.X);

                        int y1 = (int)d.Position.Y;
                        int y2 = (int)(Position.Y);
                        if (!(x1 == x2 ||
                            y1 == y2 ||
                            x1 + d.Width <= x2 ||
                            y1 + d.Height <= y2))
                        {
                            FallCutting.Add((FallingBlock)d);
                        }
                    }
                }
                else
                {
                    if (d is SolidTiles)
                    {
                        if (d.CollideCheck(this))
                        {
                            Vector2 dp = Position - d.Position;
                            if (Math.Sign(dp.X) != Math.Sign(this.CutDirection.X) || Math.Sign(dp.Y) != Math.Sign(this.CutDirection.Y))
                            {
                                breaking = true;
                            }
                        }
                    }

                }
            }

        }
        //TODO: Rewrite Cut Method for Paper
        private void CutPaper()
        {
            //check list for not colliding if so call Cut(X/Y)()
            Cutting.RemoveAll(d =>
            {
                if (!d.CollideCheck(this))
                {
                    return d.Cut(Position, CutDirection, cutSize);
                }
                return false;
            });
        }

        private void CutDreamBlocks()
        {
            if (DreamCutting.RemoveAll(d =>
            {
                if (!d.CollideCheck(this) && Scene.Contains(d))
                {
                    if (!Scene.Contains(d))
                    {
                        return true;
                    }
                    Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, CutDirection, cutSize);

                    Vector2 db1Pos = resultArray[0];
                    Vector2 db2Pos = resultArray[1];
                    int db1Width = (int)resultArray[2].X;
                    int db1Height = (int)resultArray[2].Y;

                    int db2Width = (int)resultArray[3].X;
                    int db2Height = (int)resultArray[3].Y;
                    DreamBlock d1 = new DreamBlock(db1Pos, db1Width, db1Height, null, false, false);
                    DreamBlock d2 = new DreamBlock(db2Pos, db2Width, db2Height, null, false, false);
                    if (db1Width >= 8 && db1Height >= 8)
                    {

                        Scene.Add(d1);
                    }
                    if (db2Width >= 8 && db2Height >= 8)
                    {

                        Scene.Add(d2);
                    }
                    Scene.Remove(d);
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);

                    AddParticles(d.Position, new Vector2(d.Width, d.Height));
                    return true;
                }
                return false;
            }) > 0 && fragile)
            {
                RemoveSelf();
            }
        }

        private void CutFallBlocks()
        {
            if (FallCutting.RemoveAll(d =>
             {
                 if (!d.CollideCheck(this))
                 {
                     if (!Scene.Contains(d))
                     {
                         return true;
                     }
                     Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, CutDirection, cutSize);
                     Vector2 fb1Pos = resultArray[0];
                     Vector2 fb2Pos = resultArray[1];
                     int fb1Width = (int)resultArray[2].X;
                     int fb1Height = (int)resultArray[2].Y;

                     int fb2Width = (int)resultArray[3].X;
                     int fb2Height = (int)resultArray[3].Y;

                     var tileTypeField = d.GetType().GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance);
                     char tileTypeChar = (char)tileTypeField.GetValue(d);

                     if (tileTypeChar == '1')
                     {
                         Audio.Play("event:/game/general/wall_break_dirt", Position);
                     }
                     else if (tileTypeChar == '3')
                     {
                         Audio.Play("event:/game/general/wall_break_ice", Position);
                     }
                     else if (tileTypeChar == '9')
                     {
                         Audio.Play("event:/game/general/wall_break_wood", Position);
                     }
                     else
                     {
                         Audio.Play("event:/game/general/wall_break_stone", Position);
                     }
                     if (fb1Width >= 8 && fb1Height >= 8)
                     {
                         FallingBlock fb1 = new FallingBlock(fb1Pos, tileTypeChar, fb1Width, fb1Height, false, false, true);
                         Scene.Add(fb1);


                         fb1.Triggered = true;
                         fb1.FallDelay = 0;
                     }
                     if (fb2Width >= 8 && fb2Height >= 8)
                     {
                         FallingBlock fb2 = new FallingBlock(fb2Pos, tileTypeChar, fb2Width, fb2Height, false, false, true);
                         Scene.Add(fb2);

                         fb2.Triggered = true;
                         fb2.FallDelay = 0;
                     }
                     AddParticles(d.Position, new Vector2(d.Width, d.Height));
                     Scene.Remove(d);
                     return true;
                 }
                 return false;
             }) > 0 && fragile)
            {
                RemoveSelf();
            }
        }

        private void CutKevins()
        {
            if (KevinCuttingActivationList.RemoveAll(d =>
             {
                 Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);

                 cbType.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, -CutDirection);
                 cbType.GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(d, new object[] { -CutDirection });
                 return true;
             }) > 0 && fragile)
            {
                RemoveSelf();
            }

            KevinCutting.RemoveAll(d =>
            {
                if (!d.CollideCheck(this))
                {
                    if (!Scene.Contains(d))
                    {
                        return true;
                    }
                    //get private fields
                    Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);


                    FieldInfo[] fia = cbType?.GetFields();
                    foreach (FieldInfo fiai in fia)
                    {
                        Logger.Log("Scissors", fiai.Name);
                    }
                    bool canMoveVertically = (bool)cbType?.GetField("canMoveVertically", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                    bool canMoveHorizontally = (bool)cbType?.GetField("canMoveHorizontally", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                    bool chillOut = (bool)cbType.GetField("chillOut", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);

                    var returnStack = cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                    var newReturnStack = Activator.CreateInstance(returnStack.GetType(), returnStack);

                    Vector2 crushDir = (Vector2)cbType?.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);

                    //Process private fields
                    CrushBlock.Axes axii = (canMoveVertically && canMoveHorizontally) ? CrushBlock.Axes.Both : canMoveVertically ? CrushBlock.Axes.Vertical : CrushBlock.Axes.Horizontal;

                    Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, CutDirection, cutSize);
                    Vector2 cb1Pos = resultArray[0];
                    Vector2 cb2Pos = resultArray[1];
                    int cb1Width = (int)resultArray[2].X;
                    int cb1Height = (int)resultArray[2].Y;

                    int cb2Width = (int)resultArray[3].X;
                    int cb2Height = (int)resultArray[3].Y;
                    //create cloned crushblocks + set data

                    Scene.Remove(d);
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    if (cb1Width >= 24 && cb1Height >= 24)
                    {
                        CrushBlock cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut);
                        cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb1, returnStack);
                        Scene.Add(cb1);

                        KevinCuttingActivationList.Add(cb1);
                    }
                    if (cb2Width >= 24 && cb2Height >= 24)
                    {
                        CrushBlock cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut);
                        cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb2, newReturnStack);
                        Scene.Add(cb2);
                        KevinCuttingActivationList.Add(cb2);
                    }

                    AddParticles(d.Position, new Vector2(d.Width, d.Height));
                    return true;
                }
                return false;
            });
        }

        public static Vector2[] CalcCuts(Vector2 blockPos, Vector2 blockSize, Vector2 cutPos, Vector2 cutDir, int gapWidth, int cutSize = 8)
        {
            Vector2 pos1, pos2, size1, size2;
            pos1 = pos2 = blockPos;
            size1 = new Vector2(blockSize.X, blockSize.Y);
            size2 = new Vector2(blockSize.X, blockSize.Y);
            if (cutDir.X != 0) //cut is horizontal
            {
                float delY = blockPos.Y + blockSize.Y - (cutPos.Y + gapWidth / 2);
                size2.Y = delY - Mod(delY, cutSize);
                pos2.Y = blockPos.Y + blockSize.Y - size2.Y;
                size1.Y = pos2.Y - pos1.Y - gapWidth;
                Logger.Log(LogLevel.Error, "CutDetails", String.Format("({0} {1}) ({2} {3}) ({4} {5}) ({6} {7}))", delY, Mod(delY, cutSize), size2.Y, pos2.Y, blockPos.Y, blockSize.Y, cutPos.Y, gapWidth / 2));

            }
            else //cut vertical
            {
                float delX = blockPos.X + blockSize.X - (cutPos.X + gapWidth / 2);
                size2.X = delX - Mod(delX, cutSize);
                pos2.X = blockPos.X + blockSize.X - size2.X;
                size1.X = pos2.X - pos1.X - gapWidth;
            }

            return new Vector2[] { pos1, pos2, size1, size2 };
        }

        private void AddParticles(Vector2 position, Vector2 range)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            level.ParticlesFG.Emit(ParticleTypes.Chimney, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2));
        }

        internal static void Load()
        {
        }

        internal static void Unload()
        {
        }

        public override void Render()
        {
            Vector2 placeholder = sprite.Position;
            if (!Moving && !breaking) sprite.Position += shaker.Value;
            base.Render();
            sprite.Position = placeholder;
        }

        /**
        * Cutting for vanilla entities is done on a case by case basis instead of calling the Cut(Vector2) method in a Cuttable item.
        * Due to the case by case nature of vanilla entities
        * 
        * @return: whether or not toCut was cut
        */
        private bool Cut(Entity toCut, Vector2 cutDirection, Vector2 cutPosition, int cutWidth, Type[] breaklist, Type[] ignorelist)
        {


            return false;
        }
        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

    }

}
