namespace Celeste.Mod.LylyraHelper;

public class LylyraHelperSettings : EverestModuleSettings
{

    public enum ParticleAmount
    {
        None, Light, Normal, More, WayTooMany, JustExcessive
    }
    public ParticleAmount SlicerParticles { get; set; } = ParticleAmount.Normal;

}