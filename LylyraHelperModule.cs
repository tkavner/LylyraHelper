using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Effects;
using Celeste.Mod.LylyraHelper.Triggers;
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
            AddSlicerTrigger.Load();
            typeof(ModExports).ModInterop();

            Everest.Events.Level.OnLoadBackdrop += OnLoadBackdrop;
        }

        public override void Unload()
        {
            Scissors.Unload();
            PaperHitbox.Unload();
            AddSlicerTrigger.Unload();
            Everest.Events.Level.OnLoadBackdrop -= OnLoadBackdrop;
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            _CustomEntitySpriteBank = new SpriteBank(GFX.Game, "Graphics/LylyraHelper/CustomEntitySprites.xml");
        }

        private Backdrop OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("LylyraHelper/HexagonalGodray", StringComparison.OrdinalIgnoreCase))
            {
                return new HexagonalGodray(child.Attr("color"), child.Attr("fadeColor"), child.AttrInt("numberOfRays"), child.AttrFloat("speedX"), child.AttrFloat("speedY"), child.AttrFloat("rotation"), child.AttrFloat("rotationRandomness"), child.AttrBool("hsvLerp", false));
            }
            return null;
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