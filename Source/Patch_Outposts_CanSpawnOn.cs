using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RegionsAndSocieties;
using Verse;

namespace RegionsAndSocieties.VOECP
{
    /// <summary>
    /// Postfix on <c>Outposts.Utils.CanSpawnOnWithExt</c>: a tile core's placement rules refuse is
    /// also refused for a player-built VOE outpost, with the same reason string. Applied typed —
    /// this was the last reflectively-targeted VOE patch in core.
    ///
    /// <para>The third parameter must be named <c>ps</c>, not <c>pawns</c>. Harmony binds injected
    /// parameters <b>by name</b> against the original method's signature, and VOE calls it
    /// <c>ps</c> — the mismatch made this patch fail to attach with
    /// <c>Parameter "pawns" not found in method ... CanSpawnOnWithExt</c>, which is only visible
    /// with VOE actually installed. Renaming it here is the fix; do not "tidy" it back.</para>
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Outposts_CanSpawnOn
    {
        public static bool Prepare()
        {
            if (TargetMethod() != null) return true;
            Log.Warning("[RegionsAndSocieties.VOECP] Outposts.Utils.CanSpawnOnWithExt not found on this VOE version; the placement gate patch is skipped.");
            return false;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Outposts.Utils), "CanSpawnOnWithExt");
        }

        [HarmonyPostfix]
        public static void Postfix(object ext, PlanetTile tileIdx, System.Collections.Generic.IEnumerable<Pawn> ps, ref string __result)
        {
            if (!string.IsNullOrEmpty(__result)) return;

            if (!OutpostPlacementUtility.CanPlaceOutpostAt(tileIdx.tileId, Faction.OfPlayer, out string reason))
            {
                __result = reason;
            }
        }
    }
}
