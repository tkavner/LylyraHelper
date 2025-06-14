﻿using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.LylyraHelper.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities;

[Tracked]
public class Scissors : Entity
{
    private List<Paper> Cutting = new List<Paper>();
    private Vector2 CutDirection;

    private float timeElapsed;

    private Sprite sprite;
    private bool Moving = true;
    private bool playedAudio;
    private string directionPath;


    private Dictionary<Entity, Vector2> cutStartPositions = new Dictionary<Entity, Vector2>();

    private bool fragile;

    private Level level;
    private Shaker shaker;

    private int cutSize = 32;
    private bool breaking;
    private float breakTimer;
    private Collider directionalCollider;
    private Hitbox directionalColliderHalf1;
    private Hitbox directionalColliderHalf2;
    private Vector2 initialPosition;
    private float spawnGrace = 0.5F;
    public static ParticleType scissorShards;
    private Slicer slicer;
    private float explosionCooldown;
    private bool audioFlag;
    private bool firstFrame = true;
    private bool audioFlag2;
    private EventInstance audioToken;
    private string sliceableEntityTypes;

    public Scissors(Vector2[] nodes, Vector2 direction, bool fragile = false, string sliceableEntityTypes = "") : this(nodes[0], direction, fragile, sliceableEntityTypes)
    {

    }

    public Scissors(Vector2 Position, Vector2 direction, bool fragile = false, string sliceableEntityTypes = "") : base(Position)
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
        this.sliceableEntityTypes = sliceableEntityTypes;
        if (direction.X > 0)
        {
            directionPath = "right";
            this.directionalCollider = new Hitbox(3, 10f, 7f, -5f);
            this.directionalColliderHalf1 = new Hitbox(1, 3f, 10f, -3f);
            this.directionalColliderHalf2 = new Hitbox(1, 3f, 10f, 0f);
        }
        else if (direction.X < 0)
        {
            directionPath = "left";
            this.directionalCollider = new Hitbox(3, 10f, -9f, -5f);
            this.directionalColliderHalf1 = new Hitbox(1, 3f, -10f, -3f);
            this.directionalColliderHalf2 = new Hitbox(1, 3f, -10f, 0f);
        }
        else if (direction.Y > 0)
        {
            directionPath = "down";
            this.directionalCollider = new Hitbox(10f, 3f, -5f, 7f);
            this.directionalColliderHalf1 = new Hitbox(5, 1f, -3f, 10f);
            this.directionalColliderHalf2 = new Hitbox(5, 1f, 0f, 10F);
        }
        else
        {
            this.directionalCollider = new Hitbox(10f, 3f, -5f, -9f);
            this.directionalColliderHalf1 = new Hitbox(5, 1f, -3f, -10f);
            this.directionalColliderHalf2 = new Hitbox(5, 1f, 0f, -10f);
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
        Collidable = true;
    }

    private IEnumerator Break()
    {
        while (!breaking) yield return null;
        Moving = false;
        sprite.Play("break");
        if (audioToken != null)
        {
            Audio.Stop(audioToken);
            audioToken = null;
        }
        Audio.Play("event:/Kataiser/lyra_scissors_hit", Position);
        //Partial cut non solid entities
        Collidable = false;
        yield return 0.2F;
        Audio.Play("event:/Kataiser/lyra_scissors_break", Position);
        yield return 0.55F;
        Scene.Remove(this);
        yield return null;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (audioToken != null)
        {
            Audio.Stop(audioToken);
        }
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);

        if (audioToken != null)
        {
            Audio.Stop(audioToken);
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        Add(slicer = new Slicer(CutDirection, cutSize, SceneAs<Level>(), 5, directionalCollider, settings:sliceableEntityTypes));
    }

    private void OnPlayer(Player player)
    {
        if (timeElapsed > spawnGrace && !SaveData.Instance.Assists.Invincible)
        {
            player.Die((player.Position - Position).SafeNormalize());
            Moving = false;
            sprite.Stop();
            if (audioToken != null)
            {
                Audio.Stop(audioToken);
                audioToken = null;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        float oldElapsed = timeElapsed;
        timeElapsed += Engine.DeltaTime;
        if (firstFrame)
        {
            level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
            firstFrame = false;
        }
        if (timeElapsed != oldElapsed) //check for frame advacement
        {
            explosionCooldown -= timeElapsed - oldElapsed;
            if (Moving)
            {
                    
                if (timeElapsed > spawnGrace)
                {
                    this.Position += (CutDirection).SafeNormalize() * 3;
                    sprite.CenterOrigin();
                    sprite.Visible = true;
                    sprite.Play("idle");
                    if (!playedAudio)
                    {
                        playedAudio = true;
                        if (audioToken == null) audioToken = Audio.Play("event:/Kataiser/lyra_scissors_cut_loop", Position);
                    }
                }

                if (cutStartPositions.Count > 0 && timeElapsed > spawnGrace + 0.1F)
                {
                    level.ParticlesFG.Emit(scissorShards, GetDirectionalPosition());
                }
            }
            Rectangle bounds = SceneAs<Level>().Bounds;
            if (Top >= bounds.Bottom + 32 ||
                Bottom < bounds.Top - 32 ||
                Right < bounds.Left - 32 ||
                Left > bounds.Right + 32)
            {
                if (audioToken != null)
                {
                    Audio.Stop(audioToken);
                }
                RemoveSelf();
            }
                

            if (sprite.CurrentAnimationFrame == 2 && audioFlag)
            {
                audioFlag = false;
                Audio.Play("event:/Kataiser/lyra_scissors_cut_close", Position);
            }
            audioFlag = sprite.CurrentAnimationFrame != 2;

            if (sprite.CurrentAnimationFrame == 5 && audioFlag2)
            {
                audioFlag2 = false;
                Audio.Play("event:/Kataiser/lyra_scissors_cut_open", Position);
            }
            audioFlag2 = sprite.CurrentAnimationFrame != 5;

        }
        var tempHold = Collider;

        Collider = directionalColliderHalf1;
        List<Entity> list1 = CollideAll<Solid>();
        bool flag1 = false;
        Slicer.SlicerSettings settings = slicer.settings;
        foreach (Entity entity in list1) {
            if (flag1 = flag1 || !settings.CanSlice(entity.GetType())) break;
        }

        Collider = directionalColliderHalf2;

        List<Entity> list2 = CollideAll<Solid>();
        bool flag2 = false;
        foreach (Entity entity in list2)
        {
            if (flag2 = flag2 || !settings.CanSlice(entity.GetType())) break;
        }

        Collider = tempHold;

        breaking = (flag1 && flag2) || breaking;
            
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

    public static void Load()
    {
        On.Celeste.Bumper.Update += BumperSlice;
    }

    public static void Unload()
    {
        On.Celeste.Bumper.Update -= BumperSlice;
        scissorShards = null;
    }

    private static void BumperSlice(On.Celeste.Bumper.orig_Update orig, Bumper self)
    {
        orig(self);
        if (self == null) return;
        self.CollideDo<Scissors>(
            scissors => {

                float respawnTimer = self.respawnTimer;
                bool fireMode = self.fireMode;

                if (fireMode)
                {

                    Wiggler wiggler = self.hitWiggler;
                    Vector2 hitDir = self.hitDir;
                    Vector2 vector = (scissors.Center - self.Center).SafeNormalize();
                    self.hitDir = vector;
                    wiggler.Start();
                    Audio.Play("event:/game/09_core/hotpinball_activate", self.Position);
                    self.respawnTimer = 0.6F;
                    self.SceneAs<Level>().Particles.Emit(Bumper.P_FireHit, 12, self.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                        
                }
                else if (respawnTimer <= 0f)
                {
                    if ((self.Scene as Level).Session.Area.ID == 9)
                    {
                        Audio.Play("event:/game/09_core/pinballbumper_hit", self.Position);
                    }
                    else
                    {
                        Audio.Play("event:/game/06_reflection/pinballbumper_hit", self.Position);
                    }

                    scissors.OnExplosion();
                    Vector2 vector = (scissors.Center - self.Center).SafeNormalize();
                    self.respawnTimer = 0.6F;

                    VertexLight light = self.light;
                    BloomPoint bloom = self.bloom;
                    Sprite sprite = self.sprite;
                    Sprite spriteEvil = self.spriteEvil;
                    sprite.Play("hit", restart: true);
                    spriteEvil.Play("hit", restart: true);
                    light.Visible = false;
                    bloom.Visible = false;
                    self.SceneAs<Level>().DirectionalShake(vector, 0.15f);
                    self.SceneAs<Level>().Displacement.AddBurst(self.Center, 0.3f, 8f, 32f, 0.8f);
                    self.SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, self.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                }

            });
    }

    public void OnExplosion()
    {
        if (explosionCooldown <= 0.0F) {
            explosionCooldown = 0.6F;
            sprite.Scale *= -1;
            CutDirection *= -1;
            if (CutDirection.X > 0)
            {
                directionPath = "right";
                this.directionalCollider = new Hitbox(3, 10f, 7f, -5f);
            }
            else if (CutDirection.X < 0)
            {
                directionPath = "left";
                this.directionalCollider = new Hitbox(3, 10f, -9f, -5f);
            }
            else if (CutDirection.Y > 0)
            {
                directionPath = "down";
                this.directionalCollider = new Hitbox(10f, 3f, -5f, 7f);
            }
            else
            {
                this.directionalCollider = new Hitbox(10f, 3f, -5f, -9f);
                directionPath = "up";
            }
            slicer.Flip(CutDirection, directionalCollider);
            Add(slicer = new Slicer(CutDirection, cutSize, SceneAs<Level>(), 5, directionalCollider));
        }
    }

    public override void Render()
    {
        Vector2 placeholder = sprite.Position;
        if ((!Moving && !breaking) || breaking) sprite.Position += shaker.Value;
        base.Render();
        sprite.Position = placeholder;
    }

    private static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }

}