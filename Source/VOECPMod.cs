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
    /// </summary>
    public class VOECPMod : Mod
    {
        public VOECPMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("regionsandsocieties.voecp");
            harmony.PatchAll();

            WorldObjectAdapterRegistry.Register(new VoeAdapter());
            HoldingCreatorRegistry.Register(new VoeOutpostCreator());

            Log.Message("[RegionsAndSocieties.VOECP] Registered the Vanilla Outposts Expanded adapter and outpost creator (priority 110).");
        }
    }
}
