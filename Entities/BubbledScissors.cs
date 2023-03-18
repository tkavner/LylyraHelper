using Celeste;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Entities.DashPaper;

namespace LylyraHelper.Entities
{
    public class BubbledScissors : Entity
    {

        private const float RespawnTime = 3f;

        private Sprite sprite;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool shielded;

        private bool singleUse;

        private Wiggler shieldRadiusWiggle;

        private Wiggler moveWiggle;

        private Vector2 moveWiggleDir;

        private float respawnTimer;

        public BubbledScissors(Vector2 position, bool shielded, bool singleUse)
        : base(position)
        {
            this.shielded = shielded;
            this.singleUse = singleUse;
            base.Collider = new Hitbox(20f, 20f, -10f, -10f);
            Add(new PlayerCollider(OnPlayer));
            Add(sprite = GFX.SpriteBank.Create("flyFeather"));
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.2f);
            }));
            Add(bloom = new BloomPoint(0.5f, 20f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f).Randomize());
            Add(outline = new Image(GFX.Game["objects/flyFeather/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            shieldRadiusWiggle = Wiggler.Create(0.5f, 4f);
            Add(shieldRadiusWiggle);
            moveWiggle = Wiggler.Create(0.8f, 2f);
            moveWiggle.StartZero = true;
            Add(moveWiggle);
            UpdateY();
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }

        public override void Render()
        {
            base.Render();
            if (shielded && sprite.Visible)
            {
                Draw.Circle(Position + sprite.Position, 10f - shieldRadiusWiggle.Value * 2f, Color.White, 3);
            }
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                outline.Visible = false;
                Collidable = true;
                sprite.Visible = true;
                wiggler.Start();
                Audio.Play("event:/game/06_reflection/feather_reappear", Position);
            }
        }

        private void UpdateY()
        {
            sprite.X = 0f;
            float num3 = (sprite.Y = (bloom.Y = sine.Value * 2f));
            sprite.Position += moveWiggleDir * moveWiggle.Value * -8f;
        }


        public BubbledScissors(EntityData data, Vector2 offset)
            : this(data.Position + offset, true, data.Bool("singleUse"))
        {
        }

        private void OnPlayer(Player player)
        {
            Vector2 speed = player.Speed;
            player.PointBounce(base.Center);

            if (shielded && !player.DashAttacking)
            {

                moveWiggle.Start();
                shieldRadiusWiggle.Start();
                moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
                Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                return;
            }

            Vector2 gridPosition = new Vector2(8 * (int)(player.Position.X / 8), 8 * (int)(player.Position.Y / 8));

            Vector2 direction = player.DashDir;
            Vector2 xOnly = new Vector2(direction.X, 0);
            Vector2 yOnly = new Vector2(0, direction.Y);
            var v1 = Position;
            var v2 = Position + direction * 100;
            Scissors s = new Scissors(new Vector2[] { v1, v2 }, 1, 1, 0, 1, direction, gridPosition);
            base.Scene.Add(s);
            if (player.StartStarFly())
            {
                Audio.Play(shielded ? "event:/game/06_reflection/feather_bubble_get" : "event:/game/06_reflection/feather_get", Position);
                Collidable = false;
                if (!singleUse)
                {
                    outline.Visible = true;
                    respawnTimer = 3f;
                }
            }
        }
    }
}
