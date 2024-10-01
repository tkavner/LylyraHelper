using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Code.Triggers
{
    [CustomEntity("LylyraHelper/DependencyTrigger")]
    public class DependencyTrigger : Trigger
    {
        private string dependency;
        private string flagName;
        private bool invert;

        public DependencyTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            dependency = data.Attr("dependency", "");
            flagName = data.Attr("flag", "");
            invert = data.Bool("invert", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            foreach (EverestModule module in Everest.Modules)
            {
                Logger.Log(LogLevel.Error, "LylyraHelper", "loaded module: " + module.Metadata.Name);
            }
            Logger.Log(LogLevel.Error, "LylyraHelper", "needed loaded module: " + dependency);
            if (Everest.Modules.Any(m => m.Metadata.Name == dependency))
            {
                SceneAs<Level>().Session.SetFlag(flagName, !invert);
            }
        }

    }
}
