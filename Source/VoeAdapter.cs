using RimWorld.Planet;
using RegionsAndSocieties.Integration;
using Verse;

namespace RegionsAndSocieties.VOECP
{
    /// <summary>
    /// Typed adapter for Vanilla Outposts Expanded (packageId <c>vanillaexpanded.outposts</c>).
    /// Bound against the <c>Outposts</c> assembly, which under 1.6 ships inside the Vanilla
    /// Expanded Framework — the base type every outpost derives from lives there, while the VOE
    /// mod itself supplies the concrete outposts and their defs.
    ///
    /// <para>This was the fallback profile other integrations historically leaned on, which is why
    /// its extraction went last. With every claimant typed, the ladder is explicit: Empire 100,
    /// VOE 110, VFE 120, World Domination 130 — and WD's outposts were confirmed NOT to derive
    /// from <c>Outposts.Outpost</c>, so nothing resolves through this adapter by accident.</para>
    /// </summary>
    public class VoeAdapter : WorldObjectAdapterBase
    {
        public override string AdapterId { get { return "voe"; } }

        public override string DisplayName { get { return "Vanilla Outposts Expanded"; } }

        /// <summary>Same slot as core's old reflection profile.</summary>
        public override int Priority { get { return 110; } }

        private static readonly bool Active = ModsConfig.IsActive("vanillaexpanded.outposts");

        public override bool IsActive { get { return Active; } }

        public override bool TryClassify(WorldObject obj, out WorldObjectKind kind)
        {
            kind = WorldObjectKind.Unknown;
            if (obj == null) return false;

            // The subclass-aware check covers every concrete outpost; the namespace fallback keeps
            // the old profile's behaviour for any future non-Outpost type the mod introduces.
            if (obj is Outposts.Outpost) { kind = WorldObjectKind.Outpost; return true; }
            if (obj.GetType().Namespace == "Outposts") { kind = WorldObjectKind.Outpost; return true; }

            return false;
        }

        public override bool TryGetPopulation(WorldObject obj, out int population)
        {
            population = 0;
            var outpost = obj as Outposts.Outpost;
            if (outpost == null) return false;

            population = outpost.PawnCount;
            return population >= 0;
        }
    }
}
