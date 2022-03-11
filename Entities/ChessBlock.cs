using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/ChessBlock")]
    public class ChessBlock : Solid
    {

        private enum ChessType
        {
            orthogonal, diagonal, both //aka rook, bishop, queen
        }

        private enum State
        {
            inactive, primed, active, moving, resetting, stuck, error
        }

        private enum ChessColor
        {
            black, white
        }


        private class ChessBlockTrigger : Trigger
        {

            private PlayerCollider playerCollider;

            private ChessBlock parent;

            public ChessBlockTrigger(ChessBlock parent, EntityData data, Vector2 offset) : base(data, offset)
            {
                this.parent = parent;
            }

            public override void OnEnter(Player player)
            {
                base.OnEnter(player);
                parent.OnTouch();
            }

            public override void Removed(Scene scene)
            {
                parent = null;
            }
        }

        //literally just MoveBlock.Debris
        [Pooled]
        private class Debris : Actor
        {
            private Image sprite;

            private Vector2 home;

            private Vector2 speed;

            private bool shaking;

            private bool returning;

            private float returnEase;

            private float returnDuration;

            private SimpleCurve returnCurve;

            private bool firstHit;

            private float alpha;

            private Collision onCollideH;

            private Collision onCollideV;

            private float spin;

            public Debris()
                : base(Vector2.Zero)
            {
                base.Tag = Tags.TransitionUpdate;
                base.Collider = new Hitbox(4f, 4f, -2f, -2f);
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/moveblock/debris"))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                onCollideH = delegate
                {
                    speed.X = (0f - speed.X) * 0.5f;
                };
                onCollideV = delegate
                {
                    if (firstHit || speed.Y > 50f)
                    {
                        Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
                    }
                    if (speed.Y > 0f && speed.Y < 40f)
                    {
                        speed.Y = 0f;
                    }
                    else
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                    firstHit = false;
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public Debris Init(Vector2 position, Vector2 center, Vector2 returnTo)
            {
                Collidable = true;
                Position = position;
                speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
                home = returnTo;
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                returning = false;
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * (float)Calc.Random.Choose(1, -1);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (!returning)
                {
                    if (Collidable)
                    {
                        speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                        if (!OnGround())
                        {
                            speed.Y += 400f * Engine.DeltaTime;
                        }
                        MoveH(speed.X * Engine.DeltaTime, onCollideH);
                        MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                    }
                    if (shaking && base.Scene.OnInterval(0.05f))
                    {
                        sprite.X = -1 + Calc.Random.Next(3);
                        sprite.Y = -1 + Calc.Random.Next(3);
                    }
                }
                else
                {
                    Position = returnCurve.GetPoint(Ease.CubeOut(returnEase));
                    returnEase = Calc.Approach(returnEase, 1f, Engine.DeltaTime / returnDuration);
                    sprite.Scale = Vector2.One * (1f + returnEase * 0.5f);
                }
                if ((base.Scene as Level).Transitioning)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                    sprite.Color = Color.White * alpha;
                }
                sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
            }

            public void StopMoving()
            {
                Collidable = false;
            }

            public void StartShaking()
            {
                shaking = true;
            }

            public void ReturnHome(float duration)
            {
                if (base.Scene != null)
                {
                    Camera camera = (base.Scene as Level).Camera;
                    if (base.X < camera.X)
                    {
                        base.X = camera.X - 8f;
                    }
                    if (base.Y < camera.Y)
                    {
                        base.Y = camera.Y - 8f;
                    }
                    if (base.X > camera.X + 320f)
                    {
                        base.X = camera.X + 320f + 8f;
                    }
                    if (base.Y > camera.Y + 180f)
                    {
                        base.Y = camera.Y + 180f + 8f;
                    }
                }
                returning = true;
                returnEase = 0f;
                returnDuration = duration;
                Vector2 vector = (home - Position).SafeNormalize();
                Vector2 control = (Position + home) / 2f + new Vector2(vector.Y, 0f - vector.X) * (Calc.Random.NextFloat(16f) + 16f) * Calc.Random.Facing();
                returnCurve = new SimpleCurve(Position, home, control);
            }
        }


        private Vector2 respawnPoint;
        private Vector2 moveStartPos;
        private Vector2 targetPos;

        private ChessBlockTrigger activator;
        private Wiggler wiggler;

        private ChessColor color; //Despawn if one time only. Sprites should be white if reusable, black if one time only. Block should not despawn until block hits zero on its counter.

        private int maxDashes;
        private int remainingDashes; //block cannot move if set to zero and limitedDashes is true. block loses a dash count each time the player dashes.
        private bool limitedCount;

        private ChessType type; // rook, bishop, queen
        private Vector2 lastDashDirection;
        private Vector2 lockedDashDirection;

        private State state;

        private float lerp;
        private float lerpTime = 1; //time to lerp over in seconds

        private float rampUpTime; //constant, aka alpha in supporting documents
        private float speed;

        private MTexture[] countdownTextures; //# to render, white or black (0 = white, 1 = black)
        private MTexture pieceTexture;
        private MTexture[,] backgroundTexture;
        private Level level;
        private float shakeMult = 1;

        public ChessBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            respawnPoint = targetPos = data.Position + offset;
            Logger.Log("LylyraHelper", "Chessblock spawn!");
            Logger.Log("LylyraHelper", data.Attr("color"));

            switch (data.Attr("color"))
            {
                case "Black":
                    color = ChessColor.black;
                    break;
                default:
                    color = ChessColor.white;
                    break;
            }

            switch (data.Attr("type"))
            {
                case "Rook":
                    type = ChessType.orthogonal;
                    break;
                case "Bishop":
                    type = ChessType.diagonal;
                    break;
                case "Queen":
                default:
                    type = ChessType.both;
                    break;
            }

            maxDashes = remainingDashes = data.Int("dashes", 0);
            limitedCount = maxDashes > 0 && maxDashes < 10;

            Add(new DashListener(OnDash));
            Add(new Coroutine(ResetController()));

            //Add(wiggler = Wiggler.Create(0.3f, 4f));

            //i cannot believe i cannot find a better way to do initialize the trigger. use trigger cuz the block should activate even without wallbounces.
            data.Width += 8;
            data.Height += 4;
            activator = new ChessBlockTrigger(this, data, offset + new Vector2(-4, -2));

            data.Width -= 8;
            data.Height -= 4;



            state = State.inactive;

            //textures
            countdownTextures = new MTexture[10];
            backgroundTexture = new MTexture[3, 3];

            MTexture numberTexturesUnsliced = GFX.Game["objects/LylyraHelper/chessBlock/numbers"];
            MTexture pieceTexturesUnsliced = GFX.Game["objects/LylyraHelper/chessBlock/pieces"];
            MTexture backgroundTexturesUnsliced = color == ChessColor.white ? GFX.Game["objects/LylyraHelper/chessBlock/baselight"] : GFX.Game["objects/LylyraHelper/chessBlock/basedark"];
            int colorOffset = color == ChessColor.white ? 0 : 10;

            for (int i = 0; i < 10; i++)
            {
                countdownTextures[i] = numberTexturesUnsliced.GetSubtexture(new Rectangle(i * 10, colorOffset, 10, 10));
            }
            switch (type)
            {
                case ChessType.orthogonal:
                    pieceTexture = pieceTexturesUnsliced.GetSubtexture(new Rectangle(0, colorOffset, 10, 10));
                    break;
                case ChessType.diagonal:
                    pieceTexture = pieceTexturesUnsliced.GetSubtexture(new Rectangle(10, colorOffset, 10, 10));
                    break;
                case ChessType.both:
                    pieceTexture = pieceTexturesUnsliced.GetSubtexture(new Rectangle(20, colorOffset, 10, 10));
                    break;
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    backgroundTexture[i, j] = backgroundTexturesUnsliced.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
        }



        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(activator);
            level = SceneAs<Level>();
            if (scene.Tracker.GetEntities<ChessBlock>() != null)
            {
                Logger.Log("Chess Block", scene.Tracker.GetEntities<ChessBlock>().Count > 0 ? "Block found. # Found" + scene.Tracker.GetEntities<ChessBlock>().Count : "Block missing");

            }
            else
            {
                Logger.Log("Chess Block", "Block missing");
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(activator);
        }

        public override void Update()
        {
            base.Update();
            activator.Position = Position + new Vector2(-4, -2);
            //Logger.Log("ChessBlock", lastDashDirection != null ? "X: " + lastDashDirection.X + "Y:" + lastDashDirection.Y + state.ToString(): "No Last Dash");
            float maxForwardSpeed = 360f / Vector2.Distance(moveStartPos, targetPos);
            switch (state)
            {
                case State.moving:
                    //increment lerp
                    //calculate desired speed (Calc.Approach(...), MoveTo()
                    //calculate position to move to using
                    speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
                    float oldLerp = lerp;
                    lerp = Calc.Approach(lerp, 1, speed * Engine.DeltaTime);
                    if (lerp != oldLerp) //check for frame advacement
                    {
                        Vector2 liftSpeed = (targetPos - moveStartPos) * speed;

                        if (lerp < oldLerp)
                        {
                            liftSpeed *= -1f;
                        }
                        Logger.Log("Chess Block", "speed: " + speed);
                        ;
                        if ((MoveHCheck(speed * lockedDashDirection.X)) ||
                                       (MoveVCheck(speed * lockedDashDirection.Y)))
                        {
                            if(color == ChessColor.black)
                            {
                                ResetBlock();
                            } else
                            {
                                state = State.stuck;
                            }

                            Logger.Log("Chess Block", "Collided");
                        }
                        else
                        {
                            MoveTo(Vector2.Lerp(moveStartPos, targetPos, lerp), liftSpeed);
                            if (lerp > 1) state = State.active;
                            Logger.Log("Chess Block", "No Collide");
                            //Logger.Log("ChessBlock", "Can't Move block from:" + moveStartPos.X + ", " + moveStartPos.Y + "to " + Vector2.Lerp(moveStartPos, targetPos, lerp).X + ", " + Vector2.Lerp(moveStartPos, targetPos, lerp).Y);

                        }

                    }
                    break;
                default:
                    break;
            }
            if (Shake == Vector2.Zero) shakeMult = 1;
        }
        //calculates a suitable value (called k) so that the chess block gets to the desired position in time. see supporting documents for more information.
        private float calcRampUpCoefficient()
        {
            return 0;
            //if (rampUpTime == 0)
            //{
            //    return Vector2.Distance(targetPos, moveStartPos) / lerpTime;
            //}
            //return Vector2.Distance(targetPos, moveStartPos) / (rampUpTime * (1 + 0.5F * rampUpTime));
        }

        private void OnDash(Vector2 direction)
        {
            //move and count down if active, otherwise check if this dash is valid for the block type and store the information
            Logger.Log("ChessBlock", "Dash logged!");
            Logger.Log("ChessBlock", "Dash Direction: " + direction.X + "," + direction.Y);
            Logger.Log("ChessBlock", "State:  " + state.ToString());
            switch (state)
            {
                case State.active:
                case State.moving:
                    //countdown, break if less block was at zero.
                    if (limitedCount && remainingDashes <= 0)
                    {
                        
                        //TODO implement remaining dashes
                        //reset code
                        if (color == ChessColor.black) //then this is a black piece and we should collapse it / reset it
                        {
                            ResetBlock();
                            break;
                        }
                        else //otherwise this is a white piece. do not reset. shake the block instead to indicate this.
                        {
                            state = State.stuck;
                            StartShaking(0.5F);
                            break;
                        }
                    }
                    if (remainingDashes > 0) remainingDashes--;
                    //update goal position if possible
                    if (state != State.resetting)
                    {
                        targetPos.X += Math.Sign(lockedDashDirection.X) * Width;
                        targetPos.Y += Math.Sign(lockedDashDirection.Y) * Height;
                        moveStartPos = Position;
                        lerp = 0;
                        state = State.moving;

                    }
                    break;
                case State.stuck:
                    StartShaking(0.5F);
                    shakeMult = 1.5F;
                    break;
                default:
                    break;
            }
            //switch based on type
            switch (type)
            {
                case ChessType.orthogonal:
                    if (direction.X == 0 || direction.Y == 0)
                    {
                        lastDashDirection = direction;
                        Logger.Log("ChessBlock", "orthogonal dash logged");
                        //StartShaking(0.3F);
                        if (state == State.inactive)
                        {
                            Logger.Log("ChessBlock", "Chess Block primed");
                            state = State.primed;
                        }

                        if (activator.Triggered)
                        {
                            OnTouch();
                        }
                    }
                        break;
                case ChessType.diagonal:
                    if (direction.X != 0 && direction.Y != 0)
                    {
                        lastDashDirection = direction;

                        //StartShaking(0.3F);
                        Logger.Log("ChessBlock", "diagonal dash logged");
                        if (state == State.inactive) state = State.primed;

                        if (activator.Triggered)
                        {
                            OnTouch();
                        }
                    }
                    break;
                case ChessType.both:
                    lastDashDirection = direction;

                    //StartShaking(0.3F);
                    Logger.Log("ChessBlock", "both dash logged");
                    if (state == State.inactive) state = State.primed;

                    if (activator.Triggered)
                    {
                        OnTouch();
                    }
                    break;
            }
        }

        private void ResetBlock()
        {
            state = State.resetting;
            StartShaking(0.3F);
            lockedDashDirection = Vector2.Zero;
            targetPos = moveStartPos = respawnPoint;
        }

        private IEnumerator ResetController()
        {
            while(true)
            {
                while (true)
                {
                    if (state == State.resetting) break;
                    yield return null;
                }
                Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
                speed = 0F;
                //StartShaking(0.2f);
                StopPlayerRunIntoAnimation = true;
                yield return 0.25f;
                //BreakParticles();
                List<Debris> debris = new List<Debris>();
                for (int i = 0; (float)i < Width; i += 8)
                {
                    for (int j = 0; (float)j < Height; j += 8)
                    {
                        Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                        Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, respawnPoint + vector2);
                        debris.Add(debris2);
                        Scene.Add(debris2);
                    }
                }
                DisableStaticMovers();
                Position = respawnPoint;
                Visible = (Collidable = false);
                remainingDashes = maxDashes;

                Logger.Log("ChessBlock", "Chess Block Crumbled!");
                yield return 2.2f;
                foreach (Debris item in debris)
                {
                    item.StopMoving();
                }
                while (CollideCheck<Actor>() || CollideCheck<Solid>())
                {
                    yield return null;
                }
                Collidable = true;
                EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                Coroutine component;
                Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(instance, debris)));
                Add(component);
                foreach (Debris item2 in debris)
                {
                    item2.StartShaking();
                }
                yield return 0.2f;
                foreach (Debris item3 in debris)
                {
                    item3.ReturnHome(0.65f);
                }
                yield return 0.6f;
                routine.RemoveSelf();
                foreach (Debris item4 in debris)
                {
                    item4.RemoveSelf();
                }
                Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
                Visible = true;
                EnableStaticMovers();
                speed = 0f;
                state = State.primed;
                Logger.Log("ChessBlock", "Chess Block Reformed!");
                yield return null;
            }
        }

        private IEnumerator SoundFollowsDebrisCenter(EventInstance instance, List<Debris> debris)
        {
            while (true)
            {
                instance.getPlaybackState(out var pLAYBACK_STATE);
                if (pLAYBACK_STATE == PLAYBACK_STATE.STOPPED)
                {
                    break;
                }
                Vector2 zero = Vector2.Zero;
                foreach (Debris debri in debris)
                {
                    zero += debri.Position;
                }
                zero /= (float)debris.Count;
                Audio.Position(instance, zero);
                yield return null;
            }
        }

        //whenever the block is touched
        public void OnTouch()
        {
            Logger.Log("ChessBlock", "Chess Block Touched!");
            if (state == State.primed)
            {
                Logger.Log("ChessBlock", "Chess Block activated");
                lockedDashDirection = lastDashDirection;
                Logger.Log("ChessBlock", "Locked Dash Direction: " + lockedDashDirection.X + "," + lockedDashDirection.Y);
                state = State.active;
                Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
                //StartShaking(0.6F);
            }
            else
            {
                Audio.Play("event:/game/04_cliffside/arrowblock_side_release", Position);
            }
        }
        public override void Render()
        {
            var DrawPos = Position + 0.5F * shakeMult * Shake;
            backgroundTexture[0, 0].Draw(DrawPos + new Vector2(0f, 0f), Vector2.Zero);
            backgroundTexture[2, 0].Draw(DrawPos + new Vector2(Width - 8f, 0f), Vector2.Zero);
            backgroundTexture[0, 2].Draw(DrawPos + new Vector2(0f, Height - 8f), Vector2.Zero);
            backgroundTexture[2, 2].Draw(DrawPos + new Vector2(Width - 8f, Height - 8f), Vector2.Zero);
            
            for (int i = 8; i < Width - 8; i+=8)
            {
                backgroundTexture[1, 0].Draw(DrawPos + new Vector2(i, 0f), Vector2.Zero);
                backgroundTexture[1, 2].Draw(DrawPos + new Vector2(i, Height - 8f), Vector2.Zero);
            }
            
            for (int i = 8; i < Height - 8; i+=8)
            {
                backgroundTexture[0, 1].Draw(DrawPos + new Vector2(0, i), Vector2.Zero);
                backgroundTexture[2, 1].Draw(DrawPos + new Vector2(Width - 8f, i), Vector2.Zero);
            }
            
            for (int i = 8; i < Width - 8; i+=8)
            {
                for (int j = 8; j < Height - 8; j+=8)
                {
                    backgroundTexture[1, 1].Draw(DrawPos + new Vector2(i, j), Vector2.Zero);
                }
            }
            Vector2 direction = lockedDashDirection;
            if (lockedDashDirection == Vector2.Zero) direction = lastDashDirection;
            pieceTexture.DrawCentered(DrawPos + new Vector2(Width, Height) / 2 + direction, state == State.inactive ? Color.Gray : Color.White);
            if (limitedCount) countdownTextures[remainingDashes].DrawCentered(DrawPos + new Vector2(Width, Height) / 2 + direction, state == State.inactive ? Color.LightGray : Color.White);
        }

        //lifted from the kevin blocks
        private bool MoveHCheck(float amount)
        {
            if (MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: false))
            {

                Logger.Log("ChessBlock", "MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true) was true");
                if (amount < 0f && base.Left <= (float)level.Bounds.Left)
                {
                    Logger.Log("ChessBlock", "amount < 0f && base.Left <= (float)level.Bounds.Left was true");
                    return true;
                }
                if (amount > 0f && base.Right >= (float)level.Bounds.Right)
                {

                    Logger.Log("ChessBlock", "amount > 0f && base.Right >= (float)level.Bounds.Right was true");
                    return true;
                }
                for (int i = 1; i <= 4; i++)
                {
                    for (int num = 1; num >= -1; num -= 2)
                    {
                        Vector2 vector = new Vector2(Math.Sign(amount), i * num);
                        if (!CollideCheck<Solid>(Position + vector))
                        {
                            MoveVExact(i * num);
                            MoveHExact(Math.Sign(amount));

                            Logger.Log("ChessBlock", "!CollideCheck<Solid>(Position + vector) failed");
                            return false;
                        }
                    }
                }
                return true;
            }
            Logger.Log("ChessBlock", "MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true) failed");
            return false;
        }

        //also lifted from the kevin blocks
        private bool MoveVCheck(float amount)
        {
            if (MoveVCollideSolidsAndBounds(level, amount, thruDashBlocks: false, null, checkBottom: false))
            {
                if (amount < 0f && base.Top <= (float)level.Bounds.Top)
                {
                    return true;
                }
                for (int i = 1; i <= 4; i++)
                {
                    for (int num = 1; num >= -1; num -= 2)
                    {
                        Vector2 vector = new Vector2(i * num, Math.Sign(amount));
                        if (!CollideCheck<Solid>(Position + vector))
                        {
                            MoveHExact(i * num);
                            MoveVExact(Math.Sign(amount));
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
