using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper
{
    public class LylyraHelperSettings : EverestModuleSettings
    {

        public enum ParticleAmount
        {
            None, Light, Normal, More, WayTooMany, JustExcessive
        }
        public ParticleAmount SlicerParticles { get; set; } = ParticleAmount.Normal;

    }
}
