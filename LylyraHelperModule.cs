using Celeste;
using Celeste.Mod;
using Monocle;

namespace LylyraHelper.Entities
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
            Logger.Log("LylyraHelper", "LylyraHelper Loaded!");
        }

        public override void Unload()
        {
            
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);

            _CustomEntitySpriteBank = new SpriteBank(GFX.Game, "Graphics/LylyraHelper/CustomEntitySprites.xml");
            foreach (string s in _CustomEntitySpriteBank.SpriteData.Keys) {
                Logger.Log("LylyraModule", s);
            }
        }
    }
}