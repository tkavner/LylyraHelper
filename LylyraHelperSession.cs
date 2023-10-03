using Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper
{
    public class LylyraHelperSession : EverestModuleSession
    {
        public string[] defaultSlicerSettings; //this can be made a list so we dont convert between string[] and List<> if the game wont cause race conditions but idk if it does
    }
}
