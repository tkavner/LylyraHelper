using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Components;

//component for static mover entities specified by SlicerController
public abstract class AttachedSliceableComponent : Component
{

    public AttachedSliceableComponent() : base(true, true)
    {
            
    }

    public virtual bool isDIY () { return false; }

    public abstract string GetOrientation(Entity orientableEntity);
    public abstract Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation);
    public virtual void DIY(Scene scene, StaticMover mover, Vector2 direction, Solid child1, Solid child2) { }
}