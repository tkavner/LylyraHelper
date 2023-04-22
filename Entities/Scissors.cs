﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Intefaces;
using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Entities
{
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

        private Dictionary<Entity, Vector2> cutStartPositions = new Dictionary<Entity, Vector2>();

        private bool fragile;

        private Level level;
        private Shaker shaker;

        private int cutSize = 32;
        private bool breaking;
        private float breakTimer;
        private Collider directionalCollider;
        private Vector2 initialPosition;
        private float spawnGrace = 0.5F;
        public static ParticleType scissorShards;
        private Slicer slicer;

        public Scissors(Vector2[] nodes, Vector2 direction, bool fragile = false) : this(nodes[0], direction, fragile)
        {

        }

        public Scissors(Vector2 Position, Vector2 direction, bool fragile = false) : base(Position)
        {
            //janky hackfix but I'm not really sure how to load particles
            if (scissorShards == null)
            {
                Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/scissorshard00"], 
                    GFX.Game["particles/LylyraHelper/scissorshard01"], 
                    GFX.Game["particles/LylyraHelper/scissorshard02"]);
                scissorShards = new ParticleType()
                {
                    SourceChooser = sourceChooser,
                    Color = Color.White,
                    Acceleration = new Vector2(0f, 4f),
                    LifeMin = 0.4f,
                    LifeMax = 1.2f,
                    Size = .8f,
                    SizeRange = 0.2f,
                    Direction = (float)Math.PI / 2f,
                    DirectionRange = 0.5f,
                    SpeedMin = 5f,
                    SpeedMax = 15f,
                    RotationMode = ParticleType.RotationModes.Random,
                    ScaleOut = true,
                    UseActualDeltaTime = true
                };
            }
            this.CutDirection = direction;
            if (direction.X > 0)
            {
                directionPath = "right";
                this.directionalCollider = new Hitbox(1, 6f, 10f, -5f);
            }
            else if (direction.X < 0)
            {
                directionPath = "left";
                this.directionalCollider = new Hitbox(1, 6f, -10f, -5f);
            }
            else if (direction.Y > 0)
            {
                directionPath = "down";
                this.directionalCollider = new Hitbox(10, 1f, -5f, 10f);
            }
            else
            {
                this.directionalCollider = new Hitbox(10, 1f, -6f, -10f);
                directionPath = "up";
            }
            this.Position = initialPosition = Position;

            sprite = new Sprite(GFX.Game, "objects/LylyraHelper/scissors/");
            sprite.AddLoop("spawn", "cut" + directionPath, 0.1F, new int[] { 0 });
            sprite.AddLoop("idle", "cut" + directionPath, 0.1F);
            sprite.AddLoop("break", "break" + directionPath, 0.1F);
            sprite.Play("spawn");
            Add(sprite);
            sprite.CenterOrigin();
            sprite.Visible = true;
            base.Collider = new Circle(10F);
            Add(new PlayerCollider(OnPlayer));
            this.fragile = fragile;
            Add(shaker = new Shaker());
            Add(new Coroutine(Break()));
            Depth = Depths.Enemy - 400;
        }

        private IEnumerator Break()
        {
            while (!breaking) yield return null;
            Moving = false;
            sprite.Play("break");

            //Partial cut non solid entities
            if ((Cutting.Count + DreamCutting.Count + KevinCutting.Count + KevinCuttingActivationList.Count + FallCutting.Count > 0) && timeElapsed > spawnGrace)
            {
                slicer.Slice(true);
            }
            Collidable = false;
            yield return 0.75F;
            Scene.Remove(this);
            yield return null;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            Add(slicer = new Slicer(CutDirection, cutSize, SceneAs<Level>(), 5, directionalCollider));
        }

        private void OnPlayer(Player player)
        {
            if (timeElapsed > spawnGrace)
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
                    if (timeElapsed > spawnGrace)
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
                    foreach (Solid d in base.Scene.Tracker.GetEntities<Solid>())
                    {
                        if (d is SolidTiles)
                        {
                            if (d.CollideCheck(this))
                            {
                                breaking = true;
                            }
                        }
                    }
                    if (cutStartPositions.Count > 0 && timeElapsed > spawnGrace + 0.1F)
                    {
                        level.ParticlesFG.Emit(scissorShards, GetDirectionalPosition());
                    }
                }
                Rectangle bounds = SceneAs<Level>().Bounds;
                if (Top < bounds.Bottom ||
                    Bottom > bounds.Top ||
                    Right < bounds.Left ||
                    Left > bounds.Right)
                {
                    Scene.Remove();
                }

            }
        }

        private Vector2 GetDirectionalPosition()
        {
            if (CutDirection.X > 0)
            {
                return Position + new Vector2(6, 0);
            } 
            else if (CutDirection.X < 0)
            {

                return Position + new Vector2(-6, 0);
            }
            else if (CutDirection.Y > 0)
            {
                return Position + new Vector2(0, 6);
            }
            else 
            {
                return Position + new Vector2(0, -6);
            }
        }

        private void AddParticles(Vector2 position, Vector2 range, Color color)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            level.ParticlesFG.Emit(CuttablePaper.paperScraps, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);
            
        }

        internal static void Load()
        {
            
        }

        internal static void Unload()
        {
            scissorShards = null;
        }

        public override void Render()
        {
            Vector2 placeholder = sprite.Position;
            if (!Moving && !breaking) sprite.Position += shaker.Value;
            base.Render();
            sprite.Position = placeholder;
            
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

    }

}
