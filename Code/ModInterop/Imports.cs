using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.ModInterop;

//hypothetical imports
//[ModImportName("LylyraHelper")] //uncomment if using this file
public class Imports
{
    public static Action<Type, Dictionary<string, Delegate>> RegisterSlicerActionSet;
    public static Action<Type> UnregisterSlicerAction;
        
    //Convenience methods 
    //this method handles attached static movers (like spikes and springs) for Solids.
    public static Action<DynamicData, Solid, Solid, List<StaticMover>> HandleStaticMover;

    /**helper function for Custom Solid Sliceables. Calculates the sizes and positions of a child Solid (assuming that you don’t have some weird or crazy geometry going on inside your Solid). Returns an array of 4 Vector 2s. Indices are as follows:

    0: block1Position: the position of the first (potential) new block
    1: block2Position: the position of the second (potential) new block
    2: block1Size: x = width, y=height. If width or height is less than your Solid’s minimum dimensions, block should not be spawned.
    3: block2Size: x = width, y=height. If width or height is less than your Solid’s minimum dimensions, the second block should not be spawned.
    */

    public static Func<Solid, DynamicData, Vector2[]> CalcNewBlockPosAndSize;
    public static Func<Solid, DynamicData, int, Vector2[]> CalcNewBlockPosAndSizeNonStandardTiling;

    public static Func<Entity, DynamicData> GetSlicer;
}