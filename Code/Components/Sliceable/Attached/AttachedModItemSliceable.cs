using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Components.Slicer;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables.Attached;

public class AttachedModItemSliceable : AttachedSliceableComponent
{


    public Func<Entity, string> getOrientation;
    public Func<Scene, Entity, Vector2, int, string, Entity> getNewEntity;
    public Action<Scene, StaticMover, Vector2, Solid, Solid> diy;

    public AttachedModItemSliceable(CustomAttachedSlicingActionHolder actions) : base()
    {
        getOrientation = actions.getOrientation;
        getNewEntity = actions.getNewEntity;
        diy = actions.diy;
    }
    public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
    {
        return getNewEntity(scene, original, position, desiredLength, orientation);
    }

    public override string GetOrientation(Entity orientableEntity) => getOrientation(orientableEntity);
    public override void DIY(Scene scene, StaticMover mover, Vector2 direction, Solid child1, Solid child2) => diy(scene, mover, direction, child1, child2);

    public override bool isDIY()
    {
        return diy != null;
    }
}