using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.LylyraHelper.Entities;

[CustomEntity("LylyraHelper/DynamicColorGrade")]
public class DynamicColorGrade : Entity
{
    private static bool ResetColorGrade = true;

    private static VirtualRenderTarget CGTarget;
    private static VirtualRenderTarget CGTargetTemp;

    public DynamicColorGrade(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        
        ResetColorGrade = true;
    }

    public override void Render()
    {
        base.Render();
        
        //NOTE TO SELF
        //this doesn't work yet because we are still transforming the coordinate space on the shader.
        //change that
        //also, put this somewhere else or look at maya's code cuz we cant interrupt the main draw like we are, it breaks things
        if (ResetColorGrade)
        {
            var tempTarget = Engine.Instance.GraphicsDevice.GetRenderTargets();
            Draw.SpriteBatch.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(CGTargetTemp);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            Draw.SpriteBatch.Draw(GFX.ColorGrades.GetOrDefault(this.SceneAs<Level>().Session.ColorGrade, GFX.ColorGrades["none"]).Texture.Texture_Safe, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            Engine.Instance.GraphicsDevice.SetRenderTargets(tempTarget);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            ResetColorGrade = false;
        }
        else
        {
            var tempTarget = Engine.Instance.GraphicsDevice.GetRenderTargets();
            Draw.SpriteBatch.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(CGTarget);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, LylyraHelperGFX.ColorGradeDistort);
            ApplyParameters(SceneAs<Level>(), LylyraHelperGFX.ColorGradeDistort);
            Draw.SpriteBatch.Draw(GFX.ColorGrades.GetOrDefault(this.SceneAs<Level>().Session.ColorGrade, GFX.ColorGrades["none"]).Texture.Texture_Safe, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            
            
            Engine.Instance.GraphicsDevice.SetRenderTargets(tempTarget);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, GameplayRenderer.instance.Camera.Matrix);
        }
        Draw.Rect(this.Position, 256, 16, Color.White);
        Draw.SpriteBatch.Draw(CGTarget, this.Position + new Vector2(0, 20), Color.White);
        Draw.SpriteBatch.Draw(CGTargetTemp, this.Position, Color.White);
        Draw.SpriteBatch.Draw(GameplayBuffers.Level, this.Position + new Vector2(0, 40), Color.White);
    }
    
    
    private void ApplyParameters(Level level, Effect eff)
    {
        eff.Parameters["Time"].SetValue((float) (level.TimeActive / 36f));
        eff.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180));
        eff.Parameters["CamPos"]?.SetValue(Vector2.Zero);
        eff.Parameters["Tapering"]?.SetValue(1.0f);
        eff.Parameters["RotationMatrix"]?.SetValue(Matrix.Identity);

    }
    
    private static VirtualRenderTarget RenderTarget => CGTarget;

    public static void LoadContent()
    {
        CGTarget = VirtualContent.CreateRenderTarget("DynamicColorGrade", 256, 16);
        CGTargetTemp = VirtualContent.CreateRenderTarget("DynamicColorGradeTemp", 256, 16);
    }

    public static void Load()
    {
        On.Celeste.ColorGrade.Set_MTexture_MTexture_float += ColorGradeOnSet_MTexture_MTexture_float;
    }

    private static void ColorGradeOnSet_MTexture_MTexture_float(On.Celeste.ColorGrade.orig_Set_MTexture_MTexture_float orig, 
        MTexture fromTex, MTexture toTex, float p)
    {
        orig(fromTex, toTex, p);
        if (ResetColorGrade) return;
        //now just ignore their code and replace the textures as we see fit
        if (LylyraHelperModule.Session.CursedColorGrade ||
            ((Level) Engine.Scene).Session.GetFlag("CursedColorGrade"))
        {
            Engine.Graphics.GraphicsDevice.Textures[1] = RenderTarget;
            Engine.Graphics.GraphicsDevice.Textures[2] = RenderTarget;
            ColorGrade.Effect.Parameters["percent"].SetValue(0.8f);
        }
    }


    public static void Unload()
    {
        On.Celeste.ColorGrade.Set_MTexture_MTexture_float -= ColorGradeOnSet_MTexture_MTexture_float;
        RenderTargetHelper.DisposeAndSetNull(ref CGTarget);
        RenderTargetHelper.DisposeAndSetNull(ref CGTargetTemp);
    }
}