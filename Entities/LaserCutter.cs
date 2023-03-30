﻿using Celeste;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    class LaserCutter : Entity
    {

        private Vector2 CutDirection;
        private List<Paper> Cutting = new List<Paper>();
        private List<DreamBlock> DreamCutting = new List<DreamBlock>();
        private List<FallingBlock> FallCutting = new List<FallingBlock>();
        private List<CrushBlock> KevinCutting = new List<CrushBlock>();
        private List<CrushBlock> KevinCuttingActivationList = new List<CrushBlock>();




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

            foreach (FallingBlock d in base.Scene.Tracker.GetEntities<FallingBlock>())
            {
                int x1 = (int)d.Position.X;
                int x2 = (int)(Position.X);

                int y1 = (int)d.Position.Y;
                int y2 = (int)(Position.Y);
                if (!FallCutting.Contains(d) && this.CollideCheck(d))
                {
                    if (!(x1 == x2 ||
                        y1 == y2 ||
                        x1 + d.Width <= x2 ||
                        y1 + d.Height <= y2))
                    {
                        FallCutting.Add(d);
                    }
                }
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
                if (d.GetType() != typeof(CrushBlock))
                    continue;
                int x1 = (int)d.Position.X;
                int x2 = (int)(Position.X);

                int y1 = (int)d.Position.Y;
                int y2 = (int)(Position.Y);
                if (!KevinCutting.Contains(d) && this.CollideCheck(d))
                {
                    if (!(x1 == x2 ||
                        y1 == y2 ||
                        x1 + d.Width <= x2 ||
                        y1 + d.Height <= y2))
                    {
                        KevinCutting.Add((CrushBlock)d);
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

                    }
                    if (CutDirection.X != 0)
                    {

                    }
                    return true;
                }
                return false;
            });
        }

        private void CutDreamBlocks()
        {
            DreamCutting.RemoveAll(d =>
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
            });
        }

        private void CutFallBlocks()
        {
            FallCutting.RemoveAll(d =>
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
            });
        }

        private void CutKevins()
        {
            KevinCuttingActivationList.RemoveAll(d => {
                Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);

                cbType.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, -CutDirection);
                cbType.GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(d, new object[] { -CutDirection });
                return true;
            });

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

                    if (CutDirection.Y != 0) //traveling up/down, split on width
                    {
                        cb1Height = cb2Height = (int)d.Collider.Height;
                        //find relative pos of scissors
                        Vector2 diffPos = Position - d.Position;
                        float diffX = diffPos.X;
                        //round to nearest 8
                        diffX = (float)Math.Round(diffX / 8) * 8;
                        if (diffX < 8) diffX = 8; //minimum cut size
                        cb1Width = (int)diffX;
                        cb2Width = (int)(d.Collider.Width - diffX);

                        cb2Pos += new Vector2(cb1Width, 0);
                    }
                    else
                    {
                        cb1Width = cb2Width = (int)d.Collider.Height;
                        //check for larger side
                        Vector2 diffPos = Position - d.Position;
                        float diffY = diffPos.Y;
                        //round to nearest 8
                        diffY = (float)Math.Round(diffY / 8) * 8;
                        if (diffY < 8) diffY = 8; //minimum cut size
                        cb1Height = (int)diffY;
                        cb2Height = (int)(d.Collider.Height - diffY);

                        cb2Pos += new Vector2(0, cb1Height);
                    }

                    //create cloned crushblocks + set data


                    Scene.Remove(d);
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
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    return true;
                }
                return false;
            });
        }


    }

}