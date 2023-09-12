using Celeste.Mod;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper
{
    public class LylyraHelperSession :EverestModuleSession
    {
        public bool playerCursed = false;
        public bool ignoreDash = false;
        public bool killPlayerWhenSafe = false;

        public void ResetCurse()
        {
            playerCursed = false;
            ignoreDash = false;
            killPlayerWhenSafe = false;
        }

        public void SetCurse(bool ignoreDashNew)
        {
            this.ignoreDash = ignoreDashNew;
            this.playerCursed = playerCursed || !ignoreDashNew;
        }
    }
}
