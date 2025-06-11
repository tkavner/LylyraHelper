using Celeste.Mod.Entities;
using LylyraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Entities.SecretSanta;

[CustomEntity("LylyraHelper/SS2024/UncursedRefill")]
public class UncursedRefill : Refill
{
    private Sprite sprite;
    private FieldInfo respawnTimerField;
    private bool oneUse;
    private Sprite flash;
    private Image outline;
    private float respawnTimer;

    private Color particleColor1 = Calc.HexToColor("ba3e48");
    private Color particleColor2 = Calc.HexToColor("a66096");

    private static LylyraHelperSession session => LylyraHelperModule.Session;

    public UncursedRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, false)
    {
        PlayerCollider origCollider = Get<PlayerCollider>();
        Remove(origCollider);
        Add(new PlayerCollider(OnPlayer));
        sprite = Get<Sprite>();
        respawnTimerField = typeof(Refill).GetField("respawnTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        oneUse = data.Bool("oneUse");


        sprite = Get<Sprite>();
        Remove(sprite);
        sprite.Reset(GFX.Game, "objects/LylyraHelper/ss2024/uncursedRefill/idle");
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        flash = Get<Sprite>();
        Remove(flash);
        flash.Reset(GFX.Game, "objects/LylyraHelper/ss2024/uncursedRefill/flash");
        Remove(Get<Image>());
        Add(sprite);
        Add(flash);
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        Add(outline = new Image(GFX.Game["objects/LylyraHelper/ss2024/uncursedRefill/outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
    }

    private void OnPlayer(Player player)
    {
        if (player.DashAttacking && session.playerCursed)
        {
            session.ResetCurse();
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimerField?.SetValue(this, 2.5F);
            respawnTimer = 2.5F;
            player.RefillDash();
            sprite.Visible = false;
            outline.Visible = !oneUse;

        }

    }

    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                outline.Visible = false;
            }
        }
    }

    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        SceneAs<Level>().Shake();

        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        SceneAs<Level>().ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, particleColor1, num - (float)Math.PI / 2f);
        SceneAs<Level>().ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, particleColor2, num + (float)Math.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }

    }

}