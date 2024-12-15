using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceable;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceables;
using Celeste.Mod.LylyraHelper.Code.Entities.SecretSanta;
using Celeste.Mod.LylyraHelper.Code.Entities.SS2024;
using Celeste.Mod.LylyraHelper.Code.Triggers.SS2024;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Effects;
using Celeste.Mod.LylyraHelper.Entities;
using Celeste.Mod.LylyraHelper.Triggers;
using LylyraHelper;
using LylyraHelper.Entities;
using LylyraHelper.Other;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper
{
    public class LylyraHelperModule : EverestModule
    {

        public LylyraHelperModule()
        {
            Instance = this;
        }

        public static SpriteBank SpriteBank => Instance._CustomEntitySpriteBank;
        private SpriteBank _CustomEntitySpriteBank;
        public static LylyraHelperModule Instance;
        public override Type SessionType => typeof(LylyraHelperSession);
        public static LylyraHelperSession Session => Instance._Session as LylyraHelperSession;


        public override Type SettingsType => typeof(LylyraHelperSettings);
        public static LylyraHelperSettings Settings => Instance._Settings as LylyraHelperSettings;

        public override void Load()
        {
            Logger.SetLogLevel("LylyraHelper", LogLevel.Info);
            Logger.Log("LylyraHelper", "LylyraHelper Loaded!");
            Scissors.Load();
            PaperHitbox.Load();
            AddSlicerTrigger.Load();
            Slicer.Load();
            AttachedTriggerSpikesSliceable.Load();
            typeof(ModExports).ModInterop();


            NoFastfallTrigger.Load();
            WhimsyKey.Load();
            CursedRefill.Load();
            SetFlagStatesOnRespawnTrigger.Load();
            ZipMoverSliceableComponent.Load();

            //MOD INTEROP TESTING
            /*
            Slicer.CustomSlicingActionHolder holder = new Slicer.CustomSlicingActionHolder();//test to see if Delegate can be cast to Func and back
            Dictionary<string, Delegate> map = new Dictionary<string, Delegate>
            {
                { "slice", DreamBlockSliceableComponent.Slice },
                { "activate", DreamBlockSliceableComponent.Activate },
                { "onSliceStart", DreamBlockSliceableComponent.OnSliceStart }
            };
            
            Slicer.RegisterSlicerAction(typeof(DreamBlock), map);
            */
        }

        public override void Unload()
        {
            Scissors.Unload();
            PaperHitbox.Unload();
            AddSlicerTrigger.Unload();
            Slicer.Unload();
            AttachedTriggerSpikesSliceable.Unload();
            SetFlagStatesOnRespawnTrigger.Unload();

            NoFastfallTrigger.Unload();
            WhimsyKey.Unload();
            CursedRefill.Unload();
            ZipMoverSliceableComponent.Unload();
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            FrostHelperImports.Load();
            _CustomEntitySpriteBank = new SpriteBank(GFX.Game, "Graphics/LylyraHelper/CustomEntitySprites.xml"); 
            typeof(ModExports).ModInterop();
        }


        [ModExportName("LylyraHelper")]
        private static class ModExports
        {

            public static void RegisterSlicerActionSet(Type type, Dictionary<string, Delegate> actions)
            {
                Slicer.RegisterSlicerAction(type, actions);
            }

            public static void UnregisterSlicerAction(Type type)
            {
                Slicer.UnregisterSlicerAction(type);
            }


            public static void RegisterSimpleSolidSlicerAction(Type type, Func<Entity, DynamicData, EntityData> GetEntityData, string particleColorHex, int minWidth, int minHeight, string audioPath)
            {
                Slicer.RegisterSimpleSolidSlicerAction(type, GetEntityData, particleColorHex, minWidth, minHeight, audioPath);
                Logger.Log(LogLevel.Info, "LylyraHelper", "Registered simple solid slicer action for type: " + type.FullName);
            }


            //this method handles attached static movers (like spikes) for Solids. Convenience Method.
            public static void HandleStaticMover(Scene scene, Vector2 direction, Solid block1, Solid block2, StaticMover mover)
            {
                Slicer.ModinteropHandleStaticMover(scene, direction, block1, block2, mover);
            }

            //this method handles attached static movers (like spikes) for Solids. Convenience Method.
            public static void HandleStaticMovers(Scene scene, Vector2 Direction, Solid cb1, Solid cb2, List<StaticMover> staticMovers)
            {
                Slicer.ModinteropHandleStaticMovers(scene, Direction, cb1, cb2, staticMovers);
            }

            //this method handles attached static movers (like spikes) for Solids. Convenience Method.
            public static void HandleStaticMovers(DynamicData slicerData, Solid cb1, Solid cb2, List<StaticMover> staticMovers)
            {
                Slicer slicer = slicerData.Target as Slicer;
                Slicer.ModinteropHandleStaticMovers(slicer.Scene, slicer.Direction, cb1, cb2, staticMovers);
            }

            public static Vector2[] CalcNewBlockPosAndSize(Vector2 blockPos, Vector2 blockSize, Vector2 cutPos, Vector2 cutDir, int gapWidth, int tilingSize)
            {
                return Slicer.CalcCuts(blockPos, blockSize, cutPos, cutDir, gapWidth, tilingSize);
            }
            public static Vector2[] CalcNewBlockPosAndSize(Vector2 blockPos, Vector2 blockSize, Vector2 cutPos, Vector2 cutDir, int gapWidth)
            {
                return Slicer.CalcCuts(blockPos, blockSize, cutPos, cutDir, gapWidth);
            }


            public static Vector2[] CalcNewBlockPosAndSize(Solid blockToBeCut, Vector2 cutPosition, Vector2 cutDir, int gapWidth, int tilingSize)
            {
                return Slicer.CalcCuts(blockToBeCut, cutPosition, cutDir, gapWidth, tilingSize);
            }
            public static Vector2[] CalcNewBlockPosAndSize(Solid blockToBeCut, Vector2 cutPosition, Vector2 cutDir, int gapWidth)
            {
                return Slicer.CalcCuts(blockToBeCut, cutPosition, cutDir, gapWidth);
            }


            public static Vector2[] CalcNewBlockPosAndSize(Solid blockToBeCut, DynamicData slicerData, int tilingSize)
            {
                Slicer slicer = slicerData.Target as Slicer;
                return Slicer.CalcCuts(blockToBeCut, slicer.GetDirectionalPosition(), slicer.Direction, slicer.CutSize, tilingSize);
            }
            public static Vector2[] CalcNewBlockPosAndSize(Solid blockToBeCut, DynamicData slicerData)
            {
                Slicer slicer = slicerData.Target as Slicer;
                return Slicer.CalcCuts(blockToBeCut, slicer.GetDirectionalPosition(), slicer.Direction, slicer.CutSize);
            }



            public static DynamicData GetSlicer(Entity entity)
            {
                return new DynamicData(entity.Get<Slicer>());
            }
        }

    }
}