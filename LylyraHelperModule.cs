using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Components;
using LylyraHelper.Entities;
using LylyraHelper.Other;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

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
            PaperHitbox.Load();
            typeof(ModExports).ModInterop();
        }

        public override void Unload()
        {
            Scissors.Unload();
            PaperHitbox.Unload();
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            _CustomEntitySpriteBank = new SpriteBank(GFX.Game, "Graphics/LylyraHelper/CustomEntitySprites.xml");

                


            Logger.Log("LylyraHelper", "" + _CustomEntitySpriteBank.SpriteData.Values.Count);

            foreach(string name in _CustomEntitySpriteBank.SpriteData.Keys)
            {
                Logger.Log("LylyraHelper", "" + name);
            }

        }

        [ModExportName("LylyraHelper.Slicer")]
        private static class ModExports
        {

            public static void RegisterSecondSlicerAction(Type type, Action<Entity, DynamicData> action)
            {
                Slicer.RegisterSecondFrameSlicerAction(type, action);
            }

            public static void UnregisterSecondSlicerAction(Type type)
            {
                Slicer.UnregisterSecondFrameSlicerAction(type);
            }
            public static void RegisterSlicerAction(Type type, Func<Entity, DynamicData, bool> action)
            {
                Slicer.RegisterSlicerAction(type, action);
            }

            public static void UnregisterSlicerAction(Type type)
            {
                Slicer.UnregisterSlicerAction(type);
            }

            public static void RegisterSlicerStaticHandler(Type type, Action<Entity, DynamicData> action)
            {
                Slicer.RegisterSlicerStaticHandler(type, action);
            }

            public static void UnregisterSlicerStaticHandler(Type type)
            {
                Slicer.UnregisterSlicerStaticHandler(type);
            }

            //this method handles attached static movers (like spikes) for Solids. Convenience Method.
            public static void HandleStaticMover(DynamicData dynData, Solid original, Solid cb1, Solid cb2, StaticMover mover, int minLength)
            {
                Slicer.ModinteropHandleStaticMover(dynData, original, cb1, cb2, mover, minLength);
            }
        }

    }
}