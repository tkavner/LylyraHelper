using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Entities.SS2024
{

    [Tracked]
    [CustomEntity("LylyraHelper/SS2024/WhimsyKey")] //aka SMWKey
    public class WhimsyKey : Actor 
    {

        public Vector2 Speed;

        public Holdable Hold;

        private Sprite sprite;

        private Level Level;

        private Collision onCollideH;

        private Collision onCollideV;

        private float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private Vector2 previousPosition;

        private HoldableCollider hitSeeker;

        private float hardVerticalHitSoundCooldown;

        private JumpThru keySolid;

        private Vector2 JUMPTHROUGH_OFFSET = new Vector2(-10, -8);
        private Collider doorCollider = new Hitbox(20f, 14f, -10f, -12f);
        private float leniencyGrabTimer;

        //we're don't use the StateMachine describe by MonoGame because a simple state machine is
        private enum State
        {
            Ungrabbed, Despawn, PreGrab, Buffered, Primed, Grabbed, PostDashLeniency
        }
        private State state = State.Ungrabbed;
        private bool grabbable;
        private bool optimizedKey;
        private bool optimizedFirstGrab = false;
        private bool optimizedKeyAtRest = true;

        public WhimsyKey(Vector2 position)
            : base(position)
        {
            previousPosition = position;
        }

        public WhimsyKey(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            grabbable = data.Bool("grabbable", true);
            string spritePath = data.Attr("spritePath", "objects/LylyraHelper/ss2024/smwKey/leafkey");
            optimizedKey = data.Bool("optimized", false);
            base.Depth = 100;
            base.Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = new Sprite(GFX.Game, ""));
            sprite.AddLoop("idle", spritePath, 0.1f, [0]);

            sprite.Add("destroyed", spritePath, 0.1f, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });

            sprite.Play("idle");
            sprite.SetOrigin(18, 20);
            sprite.Visible = true;
            Hold = new Holdable(0.1f);
            Add(Hold);
            Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = HitSpinner;
            Hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;

            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            base.Tag = Tags.TransitionUpdate;
            Add(new MirrorReflection());
            Add(new DashListener(OnDash));

        }

        public void OnDash(Vector2 direction)
        {
            if (direction.X != 0 && direction.Y > 0) SetState(State.Buffered);
        }



        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();

            keySolid = new JumpThru(Position + JUMPTHROUGH_OFFSET, 20, false);
            scene.Add(keySolid);
        }

        public void HandleDoors()
        {
            bool tempCollidableState = Collidable;
            Collidable = true;

            Collider tempHolder = Collider;
            Collider = doorCollider;

            List<Entity> doors = CollideAll<WhimsyDoor>();
            if (doors.Count > 0)
            {
                bool opennedDoor = false;
                foreach (WhimsyDoor door in doors)
                {
                    if (door.despawning) continue;
                    door.Open();
                    opennedDoor = true;
                    break;
                }
                if (opennedDoor)
                {

                    Scene.Remove(this);
                    Collider = tempHolder;

                    Audio.Play("event:/Kataiser/sfx/ww2_woodenkey_unlock", Level.Camera.Position + new Vector2(160f, 90f));
                    return;
                }
            }
            Collidable = tempCollidableState;
            Collider = tempHolder;
        }

        public override void Update()
        {

            base.Update();

            switch (state)
            {
                case State.Ungrabbed:
                    UngrabbedUpdate();
                    break;
                case State.Despawn:

                    UngrabbedUpdate();
                    DespawnUpdate();
                    break;

                case State.PreGrab:
                    PreGrabUpdate();
                    break;
                case State.Buffered:
                    UngrabbedUpdate();
                    BufferedUpdate();
                    break;

                case State.Primed:
                    UngrabbedUpdate();
                    PrimedUpdate();
                    break;
                case State.Grabbed:
                    GrabbedUpdate();
                    break;
                case State.PostDashLeniency:
                    UngrabbedUpdate();
                    PostDashLeniencyUpdate();
                    break;
            }

            bool tempCollidableState = Collidable; //key should be considered 
            Collidable = true;
            Collider tempHolder = Collider;
            if (state != State.Despawn && !optimizedKey) HandleDoors();
            Collidable = tempCollidableState;
            //keeping here in case this is still a thing
            /*if (player != null)
            {
                //are we actually still held? key has a habit of detatching under weird circumstances
                if (Hold.IsHeld && player.Holding != Hold) Hold.Release(new Vector2(0));
            }

            */
            //universal checks and rules here

            if (state != State.Despawn)
                foreach (SeekerBarrier barrier in base.Scene.Tracker.GetEntities<SeekerBarrier>())
                {
                    barrier.Collidable = true;
                    bool collided = CollideCheck(barrier);
                    barrier.Collidable = false;
                    if (collided)
                    {
                        SetState(State.Despawn);
                        break;
                    }
                }

            previousPosition = Position;
        }

        private void PostDashLeniencyUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            leniencyGrabTimer -= Engine.DeltaTime;
            WhimsyKey currentHolder = null;
            foreach (WhimsyKey smwKey in Scene.Tracker.GetEntities<WhimsyKey>())
            {
                if (smwKey.Hold.IsHeld)
                {
                    currentHolder = smwKey;
                    break;
                }
            }
            if (currentHolder == null)
            {
                if (Input.GrabCheck && player != null && player.Holding == null)
                {
                    Pickup(player);
                    return;
                }
            }
            if (leniencyGrabTimer <= 0)
            {
                SetState(State.Ungrabbed);
            }
        }

        private void GrabbedUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            prevLiftSpeed = Vector2.Zero;

            float f1 = Engine.DeltaTime * Calc.Clamp(player != null ? player.Speed.Length() : Speed.Length(), 1000, float.MaxValue);
            var approach = this.Position + JUMPTHROUGH_OFFSET;
            if (player != null)
            {
                approach = player.TopCenter + JUMPTHROUGH_OFFSET - new Vector2(0, 2);
            }
            if (Hold.Holder == null || (player != null && player.Holding != Hold))
            {
                SetState(State.Ungrabbed);
            }
            keySolid.MoveTo(Calc.Approach(keySolid.Position, approach, f1), new Vector2(0));

        }

        private void PrimedUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
        }

        private void BufferedUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            bool tempCollidableState = Collidable;

            Collider tempHolder = Collider;
            Collidable = true;
            Collider = Hold?.PickupCollider;
            if (player != null && CollideCheck<Player>() && player.Holding == null) SetState(State.Primed);
            Collider = tempHolder;
            Collidable = tempCollidableState;

        }

        private void PreGrabUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            prevLiftSpeed = Vector2.Zero;
            optimizedFirstGrab = optimizedKey;
            float f1 = Engine.DeltaTime * Calc.Clamp(player != null ? player.Speed.Length() : Speed.Length(), 1000, float.MaxValue);
            var approach = this.Position + JUMPTHROUGH_OFFSET;
            Collidable = false;
            keySolid.MoveTo(Calc.Approach(keySolid.Position, approach, f1), new Vector2(0));
            Collidable = true;
            if (Hold.Holder == null || (player != null && player.Holding != Hold))
            {
                SetState(State.Ungrabbed);
            }
            else if (Hold.Holder.Top > keySolid.Bottom)
            {
                SetState(State.Grabbed);
            }
        }

        private void DespawnUpdate()
        {
        }

        private void UngrabbedUpdate()
        {

            bool tempCollidableState = Collidable; //key should be considered 
            Collidable = true;
            var speedMagnitude = Speed.Length();
            if (speedMagnitude > 0)
            {
                optimizedKeyAtRest = false;
            }
            //keysolid move code
            float f1 = Engine.DeltaTime * Calc.Clamp(speedMagnitude, 10000, float.MaxValue);
            var approach = this.ExactPosition + JUMPTHROUGH_OFFSET;
            if ((keySolid.Position - approach).LengthSquared() > 0.1F)
                keySolid.MoveTo(Calc.Approach(keySolid.Position, approach, f1), LiftSpeed.SafeNormalize() * 100);

            //teleport catchup code code
            if ((Position - previousPosition).Length() > speedMagnitude * 3 && speedMagnitude != 0)
            {
                keySolid.Position = Position + JUMPTHROUGH_OFFSET;
            }


            if (OnGround())
            {
                float target2 = 0;
                if (!optimizedKeyAtRest)
                {
                    target2 = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));

                    if (speedMagnitude == 0 && target2 == 0) optimizedKeyAtRest = true;
                }
                Speed.X = Calc.Approach(Speed.X, target2, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = base.LiftSpeed;
                if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                {
                    Speed = prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                    Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                    if (Speed.X != 0f && Speed.Y == 0f)
                    {
                        Speed.Y = -60f;
                    }
                    if (Speed.Y < 0f)
                    {
                        noGravityTimer = 0.15f;
                    }
                }
                else
                {
                    prevLiftSpeed = liftSpeed;
                    if (liftSpeed.Y < 0f && Speed.Y < 0f)
                    {
                        Speed.Y = 0f;
                    }
                }
            }
            else if (Hold.ShouldHaveGravity)
            {
                float num2 = 300F;
                if (Speed.Y >= -90F)
                {
                    num2 *= 0.5f;
                }
                float num3 = (Speed.Y < 0f) ? 80F : 80f;
                Speed.X = Calc.Approach(Speed.X, 0f, 1.5F * num3 * Engine.DeltaTime);
                if (noGravityTimer > 0f)
                {
                    noGravityTimer -= Engine.DeltaTime;
                }
                else if (Level.Wind.Y < 0f)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 0f, num2 * Engine.DeltaTime);
                }
                else
                {
                    Speed.Y = Calc.Approach(Speed.Y, 90F, num2 * Engine.DeltaTime);
                }
            }
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

            if (base.Top > (float)(Level.Bounds.Bottom + 16))
            {
                RemoveSelf();
                return;
            }

            if (grabbable) Hold.CheckAgainstColliders();


            Collidable = tempCollidableState;
        }

        private void SetState(State toSet)
        {
            if (toSet == state) return;
            if (state == State.Despawn) return;
            if (toSet == State.PreGrab && !grabbable) return;
            //state setup rules
            //univeral rules
            leniencyGrabTimer = 0;

            //case by case rules
            switch (toSet)
            {
                case State.Ungrabbed:
                    keySolid.Collidable = true;
                    Hold.Holder = null;
                    break;

                case State.Despawn:
                    keySolid.Collidable = false;
                    Add(new Coroutine(DestroyKey(), true));
                    if (Hold.IsHeld)
                    {
                        Vector2 speed2 = Hold.Holder.Speed;
                        Hold.Holder.Drop();
                        Speed = speed2 * 0.333f;
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                    break;

                case State.PreGrab:

                    foreach (WhimsyKey key in Scene.Tracker.GetEntities<WhimsyKey>())
                    {
                        key.leniencyGrabTimer = 0;
                        if (key != this && key.state != State.Despawn)
                        {
                            key.SetState(State.Ungrabbed);
                        }
                    }
                    if (Hold?.Holder?.Top > keySolid.Bottom)
                    {
                        SetState(State.Grabbed);
                    }
                    else
                    {
                        keySolid.Collidable = false;
                    }
                    break;

                case State.Buffered:
                    keySolid.Collidable = true;
                    break;

                case State.Primed:
                    keySolid.Collidable = true;
                    break;

                case State.Grabbed:
                    keySolid.Collidable = true;
                    break;
                case State.PostDashLeniency:
                    keySolid.Collidable = true;
                    leniencyGrabTimer = Engine.DeltaTime * 6;
                    break;
            }
            state = toSet;
        }




        private IEnumerator DestroyKey()
        {
            Collidable = false;
            sprite.Play("destroyed");
            Audio.Play("event:/Kataiser/sfx/ww2_woodenkey_seeker", Position);

            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
            while (sprite.CurrentAnimationFrame != sprite.CurrentAnimationTotalFrames - 1)
            {
                Position += Speed * Engine.DeltaTime;
                Speed *= 0.95f;
                yield return null;
            }
            RemoveSelf();
            yield return null;
        }


        public static void Load()
        {
            On.Celeste.Player.DashEnd += Player_DashEnd;
            On.Celeste.Player.RedDashEnd += Player_RedDashEnd;
            On.Celeste.Holdable.Pickup += Holdable_Pickup;
        }

        private static void Player_RedDashEnd(On.Celeste.Player.orig_RedDashEnd orig, Player self)
        {
            orig(self);
            OnDashEnd(self);
        }

        private static void OnDashEnd(Player self)
        {
            WhimsyKey grabbed = null;
            //check key already held
            foreach (WhimsyKey smwKey in self.Scene.Tracker.GetEntities<WhimsyKey>())
            {
                if (smwKey.Hold.IsHeld)
                {
                    grabbed = smwKey;
                    break;
                }
            }
            //check if another key can be picked up
            if (grabbed == null && Input.GrabCheck)
                foreach (WhimsyKey key in self.Scene.Tracker.GetEntities<WhimsyKey>())
                {
                    if (key.state == State.Primed)
                    {
                        key.Pickup(self);
                        grabbed = key;
                        break;
                    }
                }
            //reset key states, if a key was grabbed, reset others to ungrabbed. if a key was not grabbed, make buffered keys post dash keys
            if (grabbed == null)
            {
                foreach (WhimsyKey key in self.Scene.Tracker.GetEntities<WhimsyKey>())
                {
                    if (key.state == State.Primed)
                    {
                        key.SetState(State.PostDashLeniency);
                    }
                    else
                    {
                        key.SetState(State.Ungrabbed);

                    }
                }
            }
            else
            {
                self.Scene.CollideDo<WhimsyKey>(new Rectangle((int)self.Left, (int)self.Top, (int)self.Width, (int)self.Height), (smwkey) =>
                {
                    if (smwkey != grabbed)
                    {
                        smwkey.SetState(State.Ungrabbed);
                    }

                });
            }
        }


        public static void Unload()
        {
            On.Celeste.Player.DashEnd -= Player_DashEnd;
            On.Celeste.Player.RedDashEnd -= Player_RedDashEnd;
            On.Celeste.Holdable.Pickup -= Holdable_Pickup;
        }

        private static bool Holdable_Pickup(On.Celeste.Holdable.orig_Pickup orig, Holdable self, Player player)
        {

            if (self.Entity is WhimsyKey key)
            {
                bool grabbed = false;
                if (!key.grabbable) return false;
                foreach (WhimsyKey smwKey in self.Scene.Tracker.GetEntities<WhimsyKey>())
                {
                    if (grabbed = (smwKey.Hold.IsHeld && self.Entity != smwKey)) break;
                }
                if (grabbed || key.state == State.Buffered || key.state == State.Primed)
                {
                    return false;
                }
            }
            return orig(self, player);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
        }

        private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
        {
            orig(self);
            OnDashEnd(self);
        }

        private void Pickup(Player player)
        {
            if (grabbable && player != null)
            {
                player.holdCannotDuck = true;
                keySolid.Collidable = false;
                //check the key along its path before teleporting key. This way it cannot clip through seeker barriers
                float distance = (player.Center - Position).Length();
                float progressIncrement = 1;
                if (distance > 0) progressIncrement = Math.Max(1 / distance, 0.01F);
                bool hitBarrier = false;
                for (float i = 0; i < 1; i += progressIncrement)
                {
                    Position = Vector2.Lerp(Position, player.Center, i);
                    foreach (SeekerBarrier barrier in base.Scene.Tracker.GetEntities<SeekerBarrier>())
                    {
                        barrier.Collidable = true;
                        bool collided = CollideCheck(barrier);
                        barrier.Collidable = false;
                        if (collided)
                        {
                            SetState(State.Despawn);
                            hitBarrier = true;
                            Position = keySolid.Position = player.Center;
                            previousPosition = Position;
                            return;
                        }
                    }
                }
                if (hitBarrier)
                {
                    return;
                }


                Position = keySolid.Position = player.Center;
                previousPosition = Position;
                SetState(State.PreGrab);
                Hold.Pickup(player);
            }
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (!Hold.IsHeld)
            {
                Speed = (base.Center - from).SafeNormalize(120f);
                SlashFx.Burst(base.Center, Speed.Angle());
            }
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!Hold.IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (keySolid != null) scene.Remove(keySolid);
        }


        public void HitSpinner(Entity spinner)
        {
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -150f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 135f;
                    Speed.Y = -75f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -135f;
                    Speed.Y = -75f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/Kataiser/sfx/ww2_woodenkey_wall", Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.4f;
        }

        private bool IsHardMaterial(Platform hit)
        {

            return hit.GetLandSoundIndex(this) == SurfaceIndex.Car || hit.GetLandSoundIndex(this) == SurfaceIndex.Brick
                || hit.GetLandSoundIndex(this) == SurfaceIndex.StoneBridge
                || hit.GetLandSoundIndex(this) == SurfaceIndex.Wood
                || hit.GetLandSoundIndex(this) == SurfaceIndex.AuroraGlass;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            //landsoft should be the default for unknown sounds
            if (IsHardMaterial(data.Hit))
            {
                Audio.Play("event:/Kataiser/sfx/ww2_woodenkey_landhard", Position);

            }
            else
            {
                Audio.Play("event:/Kataiser/sfx/ww2_woodenkey_landsoft", Position);
            }
            if (Speed.Y > 0f)
            {


            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }

        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }
            Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid)
        {
            if (Speed.Y == 0f)
            {
                return base.IsRiding(solid);
            }
            return false;
        }

        public override void OnSquish(CollisionData data)
        {
            base.OnSquish(data);
            if (!TrySquishWiggle(data, 3, 3) && !SaveData.Instance.Assists.Invincible)
            {
                Die();
            }
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SetState(State.PreGrab);
            this.keySolid.AddTag(Tags.Persistent);
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            SetState(State.Ungrabbed);
            Speed = new Vector2(force.X * 120F, force.Y * 120f);
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }

        public void Die()
        {
            SceneAs<Level>().Remove(this);

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider tempHold = Collider;
            Collider = doorCollider;
            Draw.HollowRect(Collider, Color.IndianRed);
            Collider = tempHold;
        }
    }
}
