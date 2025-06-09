using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Entities.SS2024
{
    [Tracked]
    public class EllipticalShockwave : Entity
    {

        private class Glob
        {
            public int positionOnLine;
            public float size;
            public float timer; //[0,timerMax)
            public float expand;
            public float timerMax;//[0.1, 1]

            public Glob(Random random, float expand, int numPoints)
            {
                positionOnLine = random.Next(0, numPoints);
                size = 1 + random.NextFloat() * random.NextFloat() * 12;
                timerMax = 0.1F + random.NextFloat() * 0.9f;
                this.expand = expand;
            }

            public static float Lifespan = 1.5F;

            public float Size()
            {
                return size * timer;
            }

            internal void Reset(Random random, float expand, int numPoints)
            {
                positionOnLine = random.Next(0, numPoints);
                size = 1 + random.NextFloat() * random.NextFloat() * 12;
                timerMax = 0.1F + random.NextFloat() * 0.9f;
                timer = 0;
                this.expand = expand;
            }
        }

        private float b;
        private float a;

        private float expand;

        private float expandRate = 0.1F;
        private float breakoutSpeed;
        private float thickness = 3;

        private MTexture shockwave;
        private Vector2[] ellipsePoints;
        private VertexPositionColor[] innerShockwaveVertecies;
        private VertexPositionColor[] outerVerts;

        private List<Glob> globsInner = new();
        private List<Glob> globsOuter = new();
        private int absoluteMaxGlobs = 4000;
        public int currentMaxGlobs { get { return (int)Math.Min(absoluteMaxGlobs, (200 + expand * expand / 4)); } }
        private Random random = new Random();

        private PlayerInteractMode mode;
        private DrawMode drawMode;

        public enum PlayerInteractMode
        {
            KILL, KNOCKBACK
        }
        public enum DrawMode
        {
            DISPLACEMENT, ENERGY_WAVE
        }

        private void UpdateShockwave()
        {
            
            expand += expandRate * Engine.DeltaTime;

            if (drawMode == DrawMode.DISPLACEMENT) return;

            if (innerShockwaveVertecies == null) innerShockwaveVertecies = new VertexPositionColor[NumVerteces];
            float transparency = 0.7F;
            Color innerShockwaveColor = new Color(0, 1, 1F, 0.5f); //half transparent cyan
            Color outerShockwaveColor = new Color(1, 1, 1F, 0.5f); //half transparent white
            if (outerVerts == null)
            {
                outerVerts = new VertexPositionColor[NumVerteces];
                for (int i = 0; i < currentMaxGlobs; i++)
                {
                    globsOuter.Add(new Glob(random, expand, numPoints));
                    globsInner.Add(new Glob(random, expand, numPoints));
                }

            }
            if (globsOuter.Count < currentMaxGlobs)
            {
                for (int i = 0; i < currentMaxGlobs - globsOuter.Count; i++)
                {
                    globsOuter.Add(new Glob(random, expand, numPoints));
                    globsInner.Add(new Glob(random, expand, numPoints));
                }

            }

            float[] globularAdditionsInner = new float[ellipsePoints.Length];
            foreach (var glob in globsInner)
            {
                if (glob.timer >= glob.timerMax)
                {
                    if (random.NextFloat() < 0.1F)
                    {
                        float innerRingSize = Math.Max(expand - thickness - thickness * 0.15F, 0);
                    }
                    glob.Reset(random, expand, numPoints);

                }
                glob.timer += Engine.DeltaTime / Glob.Lifespan;
                int points = (int)(8);
                for (int i = 0; i < points; i++)
                {
                    globularAdditionsInner[(i + glob.positionOnLine) % ellipsePoints.Length] += (float)(glob.Size() * Math.Cos(Math.PI * (i - points / 2) / points));
                }
            }
            innerRingTriangleCounter = 0;
            var cameraPosition = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < ellipsePoints.Length; i++)
            {
                Vector2 v1 = ellipsePoints[(i + 0) % ellipsePoints.Length];

                if ((v1 * expand + Position - cameraPosition).LengthSquared() > 300000) continue;
                Vector2 v2 = ellipsePoints[(i + 1) % ellipsePoints.Length];
                Vector2 v3 = ellipsePoints[(i + 2) % ellipsePoints.Length];
                float outerRingSize = expand;
                float innerRingSize = Math.Max(expand - thickness, 0);
                if (i % 2 == 1)
                {
                    v1 *= outerRingSize;
                    v2 *= innerRingSize - (0.3F * globularAdditionsInner[(i + 1) % ellipsePoints.Length]);
                    v3 *= outerRingSize;
                }
                else
                {
                    v1 *= innerRingSize - (0.3F * globularAdditionsInner[(i + 0) % ellipsePoints.Length]);
                    v2 *= outerRingSize;
                    v3 *= innerRingSize - (0.3F * globularAdditionsInner[(i + 2) % ellipsePoints.Length]);
                }

                innerShockwaveVertecies[3 * innerRingTriangleCounter + 0] = new VertexPositionColor(new Vector3(v1 + Position - SceneAs<Level>().Camera.Position, 0F), innerShockwaveColor * transparency);
                innerShockwaveVertecies[3 * innerRingTriangleCounter + 1] = new VertexPositionColor(new Vector3(v2 + Position - SceneAs<Level>().Camera.Position, 0F), innerShockwaveColor * transparency);
                innerShockwaveVertecies[3 * innerRingTriangleCounter + 2] = new VertexPositionColor(new Vector3(v3 + Position - SceneAs<Level>().Camera.Position, 0F), innerShockwaveColor * transparency);
                innerRingTriangleCounter++;
            }
            float[] globularAdditionsOuter = new float[ellipsePoints.Length];
            //glob management

            foreach (var glob in globsOuter)
            {
                if (glob.timer >= glob.timerMax)
                {
                    if (random.NextFloat() < 0.1F)
                    {
                        float innerRingSize = Math.Max(expand - thickness - thickness * 0.15F, 0);
                        SceneAs<Level>().ParticlesFG.Emit(Player.P_DashA, Position + innerRingSize * ellipsePoints[glob.positionOnLine] - ellipsePoints[glob.positionOnLine].SafeNormalize() * glob.Size(), Color.White);
                    }
                    glob.Reset(random, expand, numPoints);

                }
                glob.timer += Engine.DeltaTime / Glob.Lifespan;
                int points = (int)(12);
                for (int i = 0; i < points; i++)
                {
                    globularAdditionsOuter[(i + glob.positionOnLine) % ellipsePoints.Length] += (float)(glob.Size() * Math.Cos(Math.PI * (i - points / 2) / points));
                }
            }

            //outer ring management
            outerRingTriangleCounter = 0;
            for (int i = 0; i < ellipsePoints.Length; i++)
            {
                Vector2 v1 = ellipsePoints[(i + 0) % ellipsePoints.Length];
                if ((v1 * expand + Position - cameraPosition).LengthSquared() > 300000) continue;
                Vector2 v2 = ellipsePoints[(i + 1) % ellipsePoints.Length];
                Vector2 v3 = ellipsePoints[(i + 2) % ellipsePoints.Length];
                float outerRingSize = expand;
                float innerRingSize = Math.Max(expand - thickness - thickness * 0.33F, 0);
                if (i % 2 == 1)
                {
                    v1 *= outerRingSize;
                    v2 *= (innerRingSize - globularAdditionsOuter[(i + 1) % ellipsePoints.Length]);
                    v3 *= outerRingSize;
                }
                else
                {
                    v1 *= (innerRingSize - globularAdditionsOuter[(i + 0) % ellipsePoints.Length]);
                    v2 *= outerRingSize;
                    v3 *= (innerRingSize - globularAdditionsOuter[(i + 2) % ellipsePoints.Length]);
                }

                outerVerts[3 * outerRingTriangleCounter + 0] = new VertexPositionColor(new Vector3(v1 + Position - SceneAs<Level>().Camera.Position, 0F), outerShockwaveColor * transparency);
                outerVerts[3 * outerRingTriangleCounter + 1] = new VertexPositionColor(new Vector3(v2 + Position - SceneAs<Level>().Camera.Position, 0F), outerShockwaveColor * transparency);
                outerVerts[3 * outerRingTriangleCounter + 2] = new VertexPositionColor(new Vector3(v3 + Position - SceneAs<Level>().Camera.Position, 0F), outerShockwaveColor * transparency);
                outerRingTriangleCounter++;
            }

        }
        public int numPoints = 3000;
        private bool killPlayer;
        private int outerRingTriangleCounter;
        private int innerRingTriangleCounter;
        private Vector2 previousPlayerPos;
        private bool IgnorePlayerSpeedChecks;
        private float launchPower;
        private bool hasHitPlayer;
        private MTexture distortionTexture;
        private float distortionAlpha;

        public int NumVerteces
        {
            get { return numPoints * 3; }
        }


        public EllipticalShockwave(Vector2 Position, float a, float b, float initialSize, float expandRate, float shockwaveThickness, float breakoutSpeed, int absoluteMaxGlobs, int renderPointsOnMesh, bool ignorePlayerSpeedChecks, PlayerInteractMode mode, float launchPower, DrawMode drawMode) : base(Position)
        {
            this.b = b;
            this.a = a;
            this.expand = initialSize;
            this.expandRate = expandRate;
            this.breakoutSpeed = breakoutSpeed;
            this.IgnorePlayerSpeedChecks = ignorePlayerSpeedChecks;
            thickness = shockwaveThickness;
            Depth = Depths.Above;
            this.absoluteMaxGlobs = absoluteMaxGlobs;
            this.numPoints = renderPointsOnMesh;
            this.mode = mode;
            this.drawMode = drawMode;

            shockwave = GFX.Game["objects/ss2024/ellipticalShockwave/shockwave"];


            ellipsePoints = new Vector2[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double angle = (float)(i / (float)numPoints * 2 * Math.PI);
                ellipsePoints[i] = new Vector2((float)(b * Math.Cos(angle)), (float)(a * Math.Sin(angle)));
            }
            IgnorePlayerSpeedChecks = ignorePlayerSpeedChecks;
            this.launchPower = launchPower;

            if (drawMode == DrawMode.DISPLACEMENT)
            {
                distortionTexture = GFX.Game["util/displacementcirclehollow"];
                Add(new DisplacementRenderHook(RenderDisplacement));
            }
        }

        private void RenderDisplacement()   
        {
            distortionTexture.DrawCentered(Position, Color.White * 0.8f * distortionAlpha, new Vector2(b, a).SafeNormalize() * expand / (290F / 2));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Player player = scene?.Tracker.GetEntity<Player>();

            previousPlayerPos = player?.Position ?? Vector2.Zero;
        }
        /// <summary>
        /// 
        /// We use an alternate render method because we need to be on top of everything render wise, but we're using GFX.DrawVertices,
        /// which will execute instantly. as such we use a FG Backdrop to execute these calls after the main gameworld has been drawn
        /// </summary>
        public void RenderWave()
        {
            if (drawMode != DrawMode.ENERGY_WAVE) return;

            if (innerShockwaveVertecies != null)
            {
                GFX.DrawVertices(Matrix.Identity, outerVerts, outerRingTriangleCounter * 3);
                GFX.DrawVertices(Matrix.Identity, innerShockwaveVertecies, innerRingTriangleCounter * 3);
            }
        }

        /// <summary>
        /// 
        /// We use an alternate debug render method because we need to be on top of everything render wise, but we're using GFX.DrawVertices,
        /// which will execute instantly. as such we use a FG Backdrop to execute these calls after the main gameworld has been drawn
        /// </summary>
        public void DebugRenderWave(Camera camera)
        {

            Player player = Scene.Tracker.GetEntity<Player>();
            for (int i = 0; i < numPoints; i++)
            {

                Draw.Line(ellipsePoints[i] * expand + Position - camera.Position, ellipsePoints[(i + 1) % numPoints] * expand + Position - camera.Position, Color.Red);
                Draw.Line(ellipsePoints[i] * (expand - thickness) + Position - camera.Position, ellipsePoints[(i + 1) % numPoints] * (expand - thickness) + Position - camera.Position, Color.Red);

            }

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            for (int i = 0; i < numPoints; i++)
            {

                Draw.Line(ellipsePoints[i] * expand + Position, ellipsePoints[(i + 1) % numPoints] * expand + Position, Color.Red);
                Draw.Line(ellipsePoints[i] * (expand - thickness) + Position, ellipsePoints[(i + 1) % numPoints] * (expand - thickness) + Position, Color.Red);

            }
            /** I said show me the REAL Debug Hitbox (this hitbox is used for warped transformation calculations
            Vector2 playerPos = player.TopLeft;
            Vector2 playerSize = new Vector2(player.Width, player.Height);

            Vector2 playerTransformedPos = new Vector2((playerPos.X - Position.X) / b, (playerPos.Y - Position.Y) / a);
            Vector2 playerTransformedSize = new Vector2(playerSize.X / b, playerSize.Y / a);

            Hitbox transformedHitbox = new Hitbox(playerTransformedSize.X, playerTransformedSize.Y);
            
            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;

            player.Collider = transformedHitbox;
            player.Position = playerTransformedPos;

            Vector2 shockwaveTransformedPosition = new Vector2(0);
            Vector2 shockwaveActualPos = Position;
            Position = shockwaveTransformedPosition + new Vector2(180, 90);
            Collider = new Circle(expand);
            this.Collidable = true;
            collider.Render(camera);
            Collider = transformedHitbox;
            Position = playerTransformedPos + new Vector2(180, 90);

            Collider.Render(camera, Color.Blue);


            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;

            Position = shockwaveActualPos;
            Collidable = false;
            Collider = null;
            */

        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();

            if (drawMode == DrawMode.DISPLACEMENT)
            {
                distortionAlpha = Calc.Approach(distortionAlpha, 1f, Engine.DeltaTime * 4f);
            }
            if (player == null)
            {
                return;
            }
            if (player.Dead)
            {
                previousPlayerPos = player.Position;
                return;
            }
            if (killPlayer)
            {
                switch (mode)
                {
                    case PlayerInteractMode.KILL:
                        player.Die(new Vector2(1, 0));
                        break;
                    case PlayerInteractMode.KNOCKBACK:
                            player.Speed.X = -100f * launchPower;
                            if (player.Speed.Y > 30f)
                            {
                                player.Speed.Y = 30f;
                            }
                            if (!hasHitPlayer)
                            {
                                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                                Audio.Play("event:/game/05_mirror_temple/eye_pulse", player.Position);
                                hasHitPlayer = true;
                            }
                        killPlayer = false;
                        break;
                }
                previousPlayerPos = player.Position;
            }
            UpdateShockwave();
            if (CheckPlayerMovingInShockwaveDirection(player))
            {
                previousPlayerPos = player.Position;
                return;
            }


            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;
            float increment = (float)((player.Position - previousPlayerPos).Length() == 0 ? 1 : Math.Max(0.001, 1 / (player.Position - previousPlayerPos).Length()));
            if (player.Speed != Vector2.Zero)
            {
                if (IgnorePlayerSpeedChecks || (previousPlayerPos - player.Position).Length() <= Math.Min(400F, Math.Ceiling(player.Speed.Length() * Engine.DeltaTime))) //this checks for portal jumps and stuff, slow maps
                {

                    for (float i = 0; i <= 1; i += increment)
                    {
                        player.Position = player.Position * (1 - i) + i * previousPlayerPos;
                        if (CheckPlayerPos(player) && !SaveData.Instance.Assists.Invincible)
                        {
                            killPlayer = true;
                            player.Position = playerActualPosition;
                            break;
                        }
                        player.Position = playerActualPosition;
                    }
                }

            }
            else
            {
                if (CheckPlayerPos(player)) killPlayer = true;

            }

            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;
            previousPlayerPos = player.Position;
        }


        public bool CheckPlayerPos(Player player)
        {

            bool toReturn = false;

            Vector2 playerPos = player.TopLeft;
            Vector2 playerSize = new Vector2(player.Width, player.Height);

            Vector2 playerTransformedPos = new Vector2((playerPos.X - Position.X) / b, (playerPos.Y - Position.Y) / a);
            Vector2 playerTransformedSize = new Vector2(playerSize.X / b, playerSize.Y / a);

            Hitbox transformedHitbox = new Hitbox(playerTransformedSize.X, playerTransformedSize.Y);

            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;

            player.Collider = transformedHitbox;
            player.Position = playerTransformedPos;

            Vector2 shockwaveTransformedPosition = new Vector2(0);
            Vector2 shockwaveActualPos = Position;

            Position = shockwaveTransformedPosition;
            Collider = new Circle(expand);
            this.Collidable = true;
            if (this.CollideCheck(player))
            {
                //check if it's inside the smaller ellipse
                if (expand - thickness > 0)
                {
                    Collider = new Circle(expand - thickness);
                    if (this.CollideCheck(player))
                    {


                    }
                    else
                    {

                        player.Collider = playerActualHitbox;
                        player.Position = playerActualPosition;

                        Position = shockwaveActualPos;
                        Collidable = false;
                        Collider = null;
                        toReturn = true;
                    }
                }
                else
                {

                    player.Collider = playerActualHitbox;
                    player.Position = playerActualPosition;

                    Position = shockwaveActualPos;
                    Collidable = false;
                    Collider = null;
                    toReturn = true;
                }
            }

            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;

            Position = shockwaveActualPos;
            Collidable = false;
            Collider = null;

            return toReturn;
        }

        private bool CheckPlayerMovingInShockwaveDirection(Player play)
        {
            if (play.Position == Position) return false;
            if (Math.Max(play.Speed.Length(), (play.DashAttacking ? play.beforeDashSpeed.Length() : 0F)) <= breakoutSpeed) return false;
            Vector2 deltaPos = (play.Position - Position);
            deltaPos = new Vector2(deltaPos.X / b, deltaPos.Y / a);
            deltaPos.Normalize();
            Vector2 playerSpeed = play.Speed;
            playerSpeed.Normalize();
            return Math.Acos(Vector2.Dot(deltaPos, playerSpeed)) < Math.PI * 0.5F;
        }
    }
}
