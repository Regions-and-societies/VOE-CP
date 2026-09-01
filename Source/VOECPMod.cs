using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RegionsAndSocieties.Integration;
using Verse;

namespace RegionsAndSocieties.VOECP
{
    /// <summary>
    /// Mod entry. Loads after Regions and Societies core; this constructor is the whole
    /// integration wiring:
    /// 1. the typed read adapter joins core's registry (classification + population),
    /// 2. the typed write creator joins core's holding-creator registry (outpost seeding),
    /// 3. PatchAll applies the placement-gate postfix on VOE's establishment check.
    ///
    /// About.xml declares no hard dependency on a core edition: the two editions
    /// (<c>RegionsAndSocieties.Core</c> on Map Mode Framework, <c>RegionsAndSocieties.CoreRP2</c> on
    /// Realistic Planets 2) are mutually exclusive, and modDependencies cannot express "either of" —
    /// declaring one falsely flags the other edition's users. So core presence is checked here
    /// instead, and a missing core degrades to a warning rather than a type-load error.
    /// </summary>
    public class VOECPMod : Mod
    {
        public VOECPMod(ModContentPack content) : base(content)
        {
            if (!CoreLoaded())
            {
                Log.Warning("[RegionsAndSocieties.VOECP] Regions and Societies is not loaded — check your mod list to ensure the Regions and Societies (Realistic Planets 2) or the standard Map Mode Framework edition is active. The Vanilla Outposts Expanded integration was not applied.");
                return;
            }

            Initialize();
        }

        // Both editions ship the same "RegionsAndSocieties" assembly with an identical public API,
        // and RimWorld loads every active mod's assemblies before constructing any Mod class — so
        // scanning the domain detects whichever edition is present, regardless of load order or of
        // packageId suffixes that ModsConfig.IsActive would miss on local copies.
        private static bool CoreLoaded()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "RegionsAndSocieties") return true;
            }

            return false;
        }

        // NoInlining keeps the RegionsAndSocieties type references out of the constructor's JIT
        // scope, so a missing core reaches the warning above instead of a TypeLoadException.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Initialize()
        {
            var harmony = new Harmony("regionsandsocieties.voecp");
            harmony.PatchAll();

            WorldObjectAdapterRegistry.Register(new VoeAdapter());
            HoldingCreatorRegistry.Register(new VoeOutpostCreator());

            Log.Message("[RegionsAndSocieties.VOECP] Registered the Vanilla Outposts Expanded adapter and outpost creator (priority 110).");
        }
    }
}
