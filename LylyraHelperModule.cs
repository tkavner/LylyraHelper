using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Components;
using LylyraHelper.Entities;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities
{
    public class LylyraHelperModule : EverestModule
    {

        public LylyraHelperModule()
        {
            Instance = this;
        }

        public static SpriteBank SpriteBank => Instance._CustomEntitySpriteBank;
        private SpriteBank _CustomEntitySpriteBank;
        public  static LylyraHelperModule Instance;

        public override void Load()
        {
            Logger.SetLogLevel("LylyraHelper", LogLevel.Verbose);
            Logger.Log("LylyraHelper", "LylyraHelper Loaded!");
            Scissors.Load();
            Slicer.Load();
            PaperHitbox.Load();
        }

        public override void Unload()
        {
            Scissors.Unload();
            Slicer.Unload();
            PaperHitbox.Unload();
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            _CustomEntitySpriteBank = new SpriteBank(GFX.Game, "Graphics/LylyraHelper/CustomEntitySprites.xml");
        }
    }
}