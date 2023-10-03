using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Mod.LylyraHelper.Entities
{
    public class StarBomb : Entity
    {
        private ParticleType p_shatter;
        private ParticleType p_regen;
        private ParticleType p_glow;
        private Image outline;
        private Sprite sprite;

        public StarBomb(Vector2 position, bool oneUse) : base(position)
        {
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));


            string text;
                text = "objects/refill/";
                p_shatter = Refill.P_Shatter;
                p_regen = Refill.P_Regen;
                p_glow = Refill.P_Glow;
            Add(outline = new Image(GFX.Game[text + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, text + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
        }

        

        public StarBomb(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

        public override void Update()
        {
            base.Update();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        private void OnPlayer(Player obj)
        {
            if (sprite.visible)
            {

            }
            Add(new Coroutine(ExplosionRoutine()));
        }

        private IEnumerator ExplosionRoutine()
        {
            throw new NotImplementedException();
        }
    }
}
