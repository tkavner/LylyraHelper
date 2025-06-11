using Celeste.Mod.Entities;
using LylyraHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.LylyraHelper.Code.Entities.SecretSanta;

[CustomEntity("LylyraHelper/SS2024/CursedRefill")]
public class CursedRefill : Refill
{
    private Sprite sprite;
    private Sprite flash;
    private bool oneUse;
    private FieldInfo respawnTimerField;
    private Image outline;
    private float rsTimer;
    private static Color particleColor1 = Calc.HexToColor("888888");
    private static Color particleColor2 = Calc.HexToColor("444444");

    private static Color smokeColor = Calc.HexToColor("BBBBBB");

    private static Random particleRandom = new Random();
    private static float ChanceOfParticle = 0.25f;
    private static LylyraHelperSession session => LylyraHelperModule.Session;

    public CursedRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, false)
    {

        PlayerCollider origCollider = Get<PlayerCollider>();
        Remove(origCollider);
        Add(new PlayerCollider(OnPlayer));
        sprite = Get<Sprite>();
        Remove(sprite);
        sprite.Reset(GFX.Game, "objects/LylyraHelper/ss2024/cursedRefill/idle");
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        flash = Get<Sprite>();
        Remove(flash);
        flash.Reset(GFX.Game, "objects/LylyraHelper/ss2024/cursedRefill/flash");
        Remove(Get<Image>());
        Add(sprite);
        Add(flash);
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        Add(outline = new Image(GFX.Game["objects/LylyraHelper/ss2024/cursedRefill/outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        oneUse = data.Bool("oneUse", false);
        respawnTimerField = typeof(Refill).GetField("respawnTimer", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Update()
    {
        base.Update();
        if (rsTimer > 0f)
        {
            rsTimer -= Engine.DeltaTime;
            if (rsTimer <= 0f)
            {
                outline.Visible = false;

            }
        }
    }


    private new void OnPlayer(Player player)
    {
        session.SetCurse(IsPlayerDashing(player));
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(RefillRoutine(player)));
        respawnTimerField.SetValue(this, 2.5F);
        rsTimer = 2.5F;
        player.RefillDash();
        sprite.Visible = false;
        outline.Visible = !oneUse;
    }

    private new IEnumerator RefillRoutine(Player player)
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

    private bool IsPlayerDashing(Player player)
    {
        return player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StDreamDash || player.StateMachine.State == Player.StRedDash;
    }

    public static void Load()
    {
        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.Die += Player_Die;
        On.Celeste.LevelLoader.LoadingThread += CustomDashInitialize;
    }
    private static void CustomDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        orig(self);
        if (session != null)
        {
            session.ResetCurse();
        }
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        //add smoke particles
        orig(self);
        if (session.playerCursed && particleRandom.NextFloat() < ChanceOfParticle) self.SceneAs<Level>().ParticlesBG.Emit(ParticleTypes.Steam, 1, self.Center, self.Collider.HalfSize, smokeColor);
        if (session.killPlayerWhenSafe && PlayerCanSafelyDie(self))
        {
            self.Die(Vector2.UnitY, true);
        }
    }

    private static bool PlayerCanSafelyDie(Player player)
    {
        return player.StateMachine.State != Player.StPickup;
    }


    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        if (session.playerCursed)
        {
            session.killPlayerWhenSafe = true;

        }
        else if (session.ignoreDash)
        {
            session.SetCurse(false);
        }
        orig(self);
    }


    public static void Unload()
    {
        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.Die -= Player_Die;
        On.Celeste.LevelLoader.LoadingThread -= CustomDashInitialize;
    }

    private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        session.ResetCurse();
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }
}