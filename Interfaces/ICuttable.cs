using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Intefaces
{
    public interface ICuttable
    {
        bool Cut(Vector2 cutPosition, Vector2 direction, int gapWidth, Vector2 cutStartPosition);


    }
}
