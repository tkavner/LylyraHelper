using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.LylyraHelper.Entities;

[CustomEntity("LylyraHelper/KuwaharaBlurController")]
public class KuwaharaBlurController : Entity
{
    private string Flag;
    private bool On;
    private bool OneTime;
    private bool OneWay;
    
    public KuwaharaBlurController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Flag = data.String("flag", "");
        On = data.Bool("on", true);
        OneTime = data.Bool("oneTime", false);
        OneWay = (!data.Bool("revertable", false)) || OneTime; //if its a one time trigger, then it's one way as well
        
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        if (Flag == "") LylyraHelperModule.Session.KuwaharaBlur = On;
    }

    public override void Update()
    {
        base.Update();

        if (OneWay)
        {
            if (Flag != "" && (SceneAs<Level>().Session.GetFlag(Flag) || !OneWay))
            {
                LylyraHelperModule.Session.KuwaharaBlur = On;
                if (OneTime) SceneAs<Level>().Remove(this);
            }
        }
        else
        {
            if (Flag != "")
            {
                LylyraHelperModule.Session.KuwaharaBlur = SceneAs<Level>().Session.GetFlag(Flag);
            }
        }
    }

    private static void ApplyKuwaharaBlur(Level level)
    {
        if (!LylyraHelperModule.Session.KuwaharaBlur) return;

        var targets = Engine.Graphics.GraphicsDevice.GetRenderTargets();
        
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        
        
        LylyraHelperGFX.KuwaharaFilter.Parameters["ScreenSize"]?.SetValue(new Vector2(RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight));
        Engine.Graphics.GraphicsDevice.Textures[1] = GameplayBuffers.Light;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, LylyraHelperGFX.KuwaharaFilter, Matrix.Identity);
        Draw.SpriteBatch.Draw((Texture2D) (RenderTarget2D) GameplayBuffers.Level, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
        
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);  
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw((Texture2D) (RenderTarget2D) GameplayBuffers.TempA, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
        
        Engine.Graphics.GraphicsDevice.SetRenderTargets(targets);
    }
    
    private static void LevelOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdfld<Level>("Foreground")))
        {
            
            cursor.EmitLdarg0();
            cursor.EmitDelegate(ApplyKuwaharaBlur);
        }
    }

    public static void Load()
    {
        IL.Celeste.Level.Render += LevelOnRender;
    }

    public static void Unload()
    {
        
        IL.Celeste.Level.Render -= LevelOnRender;
    }
    
}
