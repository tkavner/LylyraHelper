﻿using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Components;
using LylyraHelper.Entities;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;

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
            typeof(ModExports).ModInterop();

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


        [ModExportName("LylyraHelper.Slicer")]
        private static class ModExports
        {
            public static void RegisterSlicerAction(Type type, Action<Entity, DynamicData> action)
            {
                Slicer.RegisterSlicerAction(type, action);
            }

            public static void UnregisterSlicerAction(Type type, Action<Entity, DynamicData> action)
            {
                Slicer.UnregisterSlicerAction(type, action);
            }
        }

    }
}