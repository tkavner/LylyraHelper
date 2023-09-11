using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{


    [CustomEntity("LylyraHelper/UncursedRefill")]
    public class UncursedRefill : Refill
    {
        private Sprite sprite;
        private FieldInfo respawnTimerField;
        private bool oneUse;

        private static LylyraHelperSession session => LylyraHelperModule.Session;

        public UncursedRefill(EntityData data, Vector2 offset) : base(data.Position, false, false)
        {
            PlayerCollider origCollider = Get<PlayerCollider>();
            Remove(origCollider);
            Add(new PlayerCollider(OnPlayer));
            sprite = Get<Sprite>();
            respawnTimerField = typeof(Refill).GetField("respawnTimer", BindingFlags.Instance | BindingFlags.NonPublic);
            oneUse = data.Bool("oneUse");
        }

        private void OnPlayer(Player player)
        {
            session.ResetCurse();

            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimerField?.SetValue(this, 2.5F);
            player.RefillDash();
            sprite.Visible = false;
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            SceneAs<Level>().Shake();

            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            SceneAs<Level>().ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            SceneAs<Level>().ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }
    }
}
