using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Helpers;
using Celeste.Mods.LylyraHelper.Intefaces;
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
        private List<Paper> Cutting = new List<Paper>();
        private Vector2 CutDirection;
        private Vector2 initialPosition;

        private float timeElapsed;
        private float lerp;
        private float lerpTime = 1F; //time to lerp over in seconds

        private double rampUpTime = 0.2F;
        private Vector2 moveStartPos;
        private Vector2 targetPos;
        private float speed;
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

        public Scissors(Vector2[] nodes, int amount, int index, float offset, float speedMult, Vector2 direction, Vector2 initialPosition, bool fragile = false) : base(nodes[0])
        {
            this.CutDirection = direction;
            if (nodes[1].X - nodes[0].X > 0)
            {
                directionPath = "right";
            }
            else if (nodes[1].X - nodes[0].X < 0)
            {
                directionPath = "left";
            }
            else if (nodes[1].Y - nodes[0].Y > 0)
            {
                directionPath = "down";
            }
            else
            {
                directionPath = "up";
            }
            this.initialPosition = initialPosition;
            moveStartPos = nodes[0];
            targetPos = nodes[1];
            Position = moveStartPos;

            sprite = new Sprite(GFX.Game, "objects/LylyraHelper/scissors/");
            sprite.AddLoop("spawn", "cut" + directionPath, 0.1F, new int[] { 0 });
            sprite.AddLoop("idle", "cut" + directionPath, 0.1F);
            sprite.Play("spawn");
            Add(sprite);
            sprite.CenterOrigin();
            sprite.Visible = true;
            base.Collider = new ColliderList(new Circle(12f), new Hitbox(30F, 8f, -15f, -4f));
            Add(new PlayerCollider(OnPlayer));
            this.fragile = fragile;
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
            }
        }

        public override void Update()
        {
            base.Update();
            float oldElapsed = timeElapsed;
            timeElapsed += Engine.DeltaTime;

            if (timeElapsed != oldElapsed && Moving) //check for frame advacement
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
                    this.Position += (targetPos - moveStartPos).SafeNormalize() * 3;
                    sprite.CenterOrigin();
                    sprite.Visible = true;
                    sprite.Play("idle");
                    if (!playedAudio)
                    {
                        playedAudio = true;
                    }
                }

                AddEntititesToLists();

                CutPaper();
                CutDreamBlocks();
                CutKevins();
                CutFallBlocks();
            }
        }

        private void AddEntititesToLists()
        {
            //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for sppeed)
            foreach (Paper d in base.Scene.Tracker.GetEntities<DashPaper>())
            {
                if (!Cutting.Contains(d)) if (this.CollideCheck(d)) Cutting.Add(d);
            }

            foreach (Paper d in base.Scene.Tracker.GetEntities<DeathNote>())
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
                    if (this.CollideCheck(d))
                    {
                        Scene.Remove(d);
                        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    }
                }
            }

        }

        private void CutPaper()
        {
            //check list for not colliding if so call Cut(X/Y)()
            Cutting.RemoveAll(d =>
            {
                if (!d.CollideCheck(this))
                {
                    if (CutDirection.Y != 0)
                    {
                        d.CutY(new Hole(initialPosition), CutDirection);
                    }
                    if (CutDirection.X != 0)
                    {
                        d.CutX(new Hole(initialPosition), CutDirection);
                    }
                    return true;
                }
                return false;
            });
        }

        private void CutDreamBlocks()
        {
            if (DreamCutting.RemoveAll(d =>
            {
                if (!d.CollideCheck(this))
                {
                    Vector2 d1Position = d.Position;
                    float d1Width = d.Width;
                    float d1Height = d.Height;
                    if (CutDirection.Y != 0)
                    {
                        d1Height = d.Height;
                        //check for larger side
                        if (Math.Abs(d.Position.X - this.Position.X) > Math.Abs(d.Position.X + d.Width - this.Position.X))
                        {
                            d1Width = Math.Abs(d.Position.X - this.Position.X);
                        }
                        else
                        {
                            d1Width = Math.Abs(d.Position.X + d.Width - this.Position.X);
                            d1Position.X = this.Position.X;
                        }
                    }
                    if (CutDirection.X != 0)
                    {
                        d1Height = d.Width;
                        //check for larger side
                        if (Math.Abs(d.Position.Y - this.Position.Y) > Math.Abs(d.Position.Y + d.Height - this.Position.Y))
                        {
                            d1Height = Math.Abs(d.Position.Y - this.Position.Y);
                        }
                        else
                        {
                            d1Height = Math.Abs(d.Position.Y + d.Height - this.Position.Y);
                            d1Position.Y = this.Position.Y;
                        }
                    }
                    Logger.Log("Scissors", "test");
                    DreamBlock d1 = new DreamBlock(d1Position, d1Width, d1Height, null, false, false);

                    Scene.Add(d1);
                    Scene.Remove(d);
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
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
                     Vector2 d1Position = d.Position;
                     float d1Width = d.Width;
                     float d1Height = d.Height;
                     if (CutDirection.Y != 0)
                     {
                         d1Height = d.Height;
                         //check for larger side
                         if (Math.Abs(d.Position.X - this.Position.X) > Math.Abs(d.Position.X + d.Width - this.Position.X))
                         {
                             d1Width = Math.Abs(d.Position.X - this.Position.X);
                         }
                         else
                         {
                             d1Width = Math.Abs(d.Position.X + d.Width - this.Position.X);
                             d1Position.X = this.Position.X;
                         }
                     }
                     if (CutDirection.X != 0)
                     {
                         d1Height = d.Width;
                         //check for larger side
                         if (Math.Abs(d.Position.Y - this.Position.Y) > Math.Abs(d.Position.Y + d.Height - this.Position.Y))
                         {
                             d1Height = Math.Abs(d.Position.Y - this.Position.Y);
                         }
                         else
                         {
                             d1Height = Math.Abs(d.Position.Y + d.Height - this.Position.Y);
                             d1Position.Y = this.Position.Y;
                         }
                     }
                     Logger.Log("Scissors", "test");



                     d.Collider = new Hitbox(d1Width, d1Height);


                     var tiles = d.GetType().GetField("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
                     var tileType = d.GetType().GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance);
                     char tileTypeChar = (char)tileType.GetValue(d);

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
                     TileGrid t = GFX.FGAutotiler.GenerateBox(tileTypeChar, (int)d1Width / 8, (int)d1Height / 8).TileGrid;
                     d.Remove((Component)tiles.GetValue(d));
                     d.Position = d1Position;
                     tiles.SetValue(d, t);
                     d.Add(t);

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
                     //Dimension Checks

                     //make clone crushblocks

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

                     Vector2 cb1Pos = d.Position;
                     Vector2 cb2Pos = d.Position;
                     int cb1Width, cb1Height, cb2Width, cb2Height;

                     Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, CutDirection, 8);

                     if (CutDirection.Y != 0) //traveling up/down, split on width
                     {
                         cb1Height = cb2Height = (int)d.Collider.Height;
                         //find relative pos of scissors
                         Vector2 diffPos = Position - d.Position;
                         float diffX = diffPos.X;
                         //round to nearest 8
                         diffX = (float)Math.Round(diffX / 8) * 8;
                         if (diffX < 8) diffX = 8; //minimum cut size
                         cb1Width = (int)(diffX - Collider.Width / 2);
                         cb2Width = (int)(d.Collider.Width - diffX) - (int)(16);

                         cb2Pos += new Vector2(cb1Width + 32, 0);
                     }
                     else
                     {
                         cb1Width = cb2Width = (int)d.Collider.Width;
                         //check for larger side
                         Vector2 diffPos = Position - d.Position;
                         float diffY = diffPos.Y;
                         //round to nearest 8
                         diffY = (float)Math.Round(diffY / 8) * 8;
                         if (diffY < 8) diffY = 8; //minimum cut size
                         cb1Height = (int)diffY - (int)Collider.Height / 2;
                         cb2Height = (int)(d.Collider.Height - diffY) - (int)16;
                         cb2Pos += new Vector2(0, cb1Height + 24);
                     }

                     //create cloned crushblocks + set data


                     Scene.Remove(d);
                     Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                     if (cb1Width >= 24 && cb1Height >= 24)
                     {
                         CrushBlock cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut);
                         cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb1, returnStack);
                         Scene.Add(cb1);

                         KevinCuttingActivationList.Add(cb1);
                         //TODO: Add particles for the poofing / reveal of new kevins
                     }
                     else
                     {
                         //TODO: do not spawn, instead have particles show it exploding
                     }

                     if (cb2Width >= 24 && cb2Height >= 24)
                     {
                         CrushBlock cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut);
                         cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb2, newReturnStack);
                         Scene.Add(cb2);
                         KevinCuttingActivationList.Add(cb2);

                         //TODO: Add particles for the poofing / reveal of new kevins
                     }
                     else
                     {
                         //TODO: do not spawn, instead have particles show it exploding
                     }
                     return true;
                 }
                 return false;
             });
        }

        private static Vector2[] CalcCuts(Vector2 blockPos, Vector2 blockSize, Vector2 cutPos, Vector2 cutDir, int gapWidth)
        {
            Vector2 pos1, pos2, size1, size2;
            pos1 = pos2 = blockPos;
            size1 = size2 = blockSize;
            if (cutDir.X != 0) //cut is horizontal
            {
                float x = pos1.Y + blockSize.Y;
                float r1 = Mod(x, 8F);
                int q1 = (int)((x - r1) / 8);

                float ys = cutPos.Y + gapWidth / 2 + r1;
                float r2 = Mod(ys, 8F);
                int q2 = (int)((ys - r2) / 8);

                float x3 = pos1.Y;
                float r3 = Mod(x, 8F); //r1 should = r3
                int q3 = (int)((x3 - r3) / 8);

                float x4 = cutPos.Y - gapWidth / 2 + r3;
                float r4 = Mod(r3, 8F);
                int q4 = (int)((x4 - r4) / 8);

                pos2.Y = 8 * (q2 - q1) + r1;

                size1.Y = 8 * (q4 - q3);

                size2.Y = blockPos.Y + blockSize.Y - pos2.Y;
            } else //cut vertical
            {
                float x = pos1.X + blockSize.X;
                float r1 = Mod(x, 8F);
                int q1 = (int)((x - r1) / 8);

                float ys = cutPos.X + gapWidth / 2 + r1;
                float r2 = Mod(ys, 8F);
                int q2 = (int)((ys - r2) / 8);

                float x3 = pos1.X;
                float r3 = Mod(x, 8F); //r1 should = r3
                int q3 = (int)((x3 - r3) / 8);

                float x4 = cutPos.X - gapWidth / 2 + r3;
                float r4 = Mod(r3, 8F);
                int q4 = (int)((x4 - r4) / 8);

                pos2.X = 8 * (q2 - q1) + r1;

                size1.X = 8 * (q4 - q3);

                size2.X = blockPos.X + blockSize.X - pos2.X;
            }



            return new Vector2[] { pos1, pos2, size1, size2 };
        }

        private void AddParticles(Vector2 position, Vector2 range)
        {
            int numParticles = (int)(range.X * range.Y); //proportional to the area to cover
            level.Particles.Emit(ParticleTypes.Steam, numParticles, position, range);
        }

        internal static void Load()
        {
        }

        internal static void Unload()
        {
        }
        /**
        * Cutting for vanilla entities is done on a case by case basis instead of calling the Cut(Vector2) method in a Cuttable item.
        * Due to the case by case nature of vanilla entities
        * 
        * @return: whether or not toCut was cut
        */
        private bool Cut(Entity toCut, Vector2 cutDirection, Vector2 cutPosition, int cutWidth, Type[] breaklist, Type[] ignorelist)
        {
            if (toCut is Cuttable cuttable)
            {
                return cuttable.Cut(cutDirection, cutPosition, cutWidth);
            }
            else //check for vanilla entity
            {

            }

            return false;
        }
        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

    }

}
