using Celeste.Mod.LylyraHelper.Entities;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents
{
    public class PaperComponent : Component
    {
        internal Paper Parent;
        public PaperComponent(Paper Parent) : base(true, true) { this.Parent = Parent; }

        public override void Render()
        {
            base.Render();
        }

    }
}