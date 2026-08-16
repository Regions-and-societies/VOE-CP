using System.Text;
using LudeonTK;
using RimWorld.Planet;
using RegionsAndSocieties.Integration;
using Verse;

namespace RegionsAndSocieties.VOECP
{
    /// <summary>
    /// Debug validation for the patch (per the workspace debug-command gate): dump every VOE
    /// outpost with its resolved kind and population — including any object from another mod that
    /// resolves through this adapter, so the old fallback-profile behaviour is observable.
    /// </summary>
    public static class DebugActions_VOECP
    {
        [DebugAction("Regions and Societies", "R&S VOE-CP: world-object dump", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap | AllowedGameStates.PlayingOnWorld)]
        private static void VoeObjectDump()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- VOE-CP world-object dump ---");
            int count = 0;
            var objects = Find.WorldObjects != null ? Find.WorldObjects.AllWorldObjects : null;
            if (objects != null)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    WorldObject obj = objects[i];
                    if (!(obj is Outposts.Outpost outpost)) continue;

                    count++;
                    WorldObjectKind kind;
                    WorldObjectAdapterRegistry.TryClassify(obj, out kind);
                    int pop;
                    bool hasPop = WorldObjectAdapterRegistry.TryGetPopulation(obj, out pop);
                    sb.AppendLine($"  {obj.GetType().FullName,-44} tile={obj.Tile,-7} kind={kind,-10} pop={(hasPop ? pop.ToString() : "-"),-5} faction={obj.Faction?.Name ?? "none"}");
                }
            }
            sb.AppendLine($"{count} VOE outpost(s).");
            Log.Message(sb.ToString());
        }
    }
}
