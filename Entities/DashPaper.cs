﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/DashPaper")]
    public class DashPaper : CuttablePaper
    {
        private bool spawnScissors;

        public DashPaper(Vector2 position, int width, int height, bool safe, bool trapped = true, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew")
        : base(position, width, height, safe, texture)
        {
            thisType = this.GetType();
            this.spawnScissors = true;
        }

        public DashPaper(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, data.Bool("trapped", true), false)
        {

        }

        public override void Update()
        {
            base.Update();
        }

        internal override void OnDash(Vector2 direction)
        {
            if (CanActivate())
            {
                Audio.Play("event:/char/madeline/jump");
                Activate(direction);
            }
        }

        internal virtual void Activate(Vector2 direction)
        {
            Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
            Player p = base.Scene.Tracker.GetEntity<Player>();
            var session = SceneAs<Level>().Session;
            session.Inventory.Dashes = p.MaxDashes;
            p.Dashes = p.MaxDashes;
            
            if (spawnScissors)
            {
                Vector2 gridPosition = new Vector2(8 * (int)(p.Position.X / 8), 8 * (int)(p.Position.Y / 8));

                Vector2 xOnly = new Vector2(direction.X, 0);
                Vector2 yOnly = new Vector2(0, direction.Y);
                if (direction.Y != 0)
                {
                    var v1 = new Vector2(gridPosition.X, Position.Y + Height);
                    var v2 = new Vector2(gridPosition.X, Position.Y + 0);
                    Scissors s;
                    if (direction.Y < 0)
                    {
                        s = new Scissors(new Vector2[] { v1, v2 }, yOnly, false);
                    }
                    else
                    {
                        s = new Scissors(new Vector2[] { v2, v1 }, yOnly, false);
                    }
                    base.Scene.Add(s);
                }
                if (direction.X != 0)
                {
                    var v1 = new Vector2(Position.X + Width, gridPosition.Y);
                    var v2 = new Vector2(Position.X, gridPosition.Y);
                    Scissors s;
                    if (direction.X < 0)
                    {
                        s = new Scissors(new Vector2[] { v1, v2 }, xOnly, false);
                    }
                    else
                    {
                        s = new Scissors(new Vector2[] { v2, v1 }, xOnly, false);
                    }
                    base.Scene.Add(s);
                }
            }
        }

        public bool CanActivate()
        {
            Player p = base.Scene.Tracker.GetEntity<Player>();
            if (this.CollideCheck<Player>())
            {

                Vector2[] playerPointsToCheck = new Vector2[] {
                    (p.ExactPosition - this.Position),
                    (p.ExactPosition + new Vector2(p.Width, 0) - this.Position),
                    (p.ExactPosition + new Vector2(p.Width, -p.Height) - this.Position),
                    (p.ExactPosition + new Vector2(0, -p.Height) - this.Position) };

                foreach (Vector2 v in playerPointsToCheck)
                {
                    int x = (int)v.X;
                    int y = (int)v.Y;
                    if (x >= 0 && y >= 0 && x < (int)Width && y < (int)Height)
                    {
                        if (!this.skip[x / 8, y / 8])
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }

    }
}