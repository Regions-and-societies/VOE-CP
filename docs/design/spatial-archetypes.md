# Spatial archetype placement — design spec

Milestone **0.2.0 Living Outposts**. Tracks VOE-CP [#3] and its Core-MMF companion.
Feature branch: `feature/spatial-archetypes`.

This spec decides **what** VOE outpost an AI faction seeds, **where**, and **why** —
replacing the terrain-only choice with a model that also reads a tile's position in the
faction's territory and the faction's own character.

## 1. The VOE 1.6 outpost roster

Every concrete outpost, what it projects on the map as a faction holding, and its hard gates
(from the VOE defs: `Defs/WorldObjectDefs/Outposts.xml`, plus the Factory add-on module).

| VOE def | Reads as | Hard terrain gate | Notes |
|---|---|---|---|
| `Outpost_Farming` | worked cropland | **not** Desert/ExtremeDesert; needs growing season | Plants 10 |
| `Outpost_Hunting` | hunting camp | — (wildlife reads best) | Animals/Shooting |
| `Outpost_Logging` | timber camp | **not** Desert; forested | MinPawns 3 |
| `Outpost_Mining` | mine | **Hills or Mountainous only** | Mining 10 |
| `Outpost_Drilling` | chemfuel derrick | **Desert/ExtremeDesert only** | Construction 20 |
| `Outpost_Trading` | trading post | — | Social 10 |
| `Outpost_Production` | workshop | — | Crafting 10 |
| `Outpost_Science` | research station | — | Intellectual 30 |
| `Outpost_Town` | small town | **3 settlements within 10 hexes** | MinPawns 5 |
| `Outpost_Scavenging` | scrapper camp | — | scouts nearby ruins |
| `Outpost_Factory` | mech-tech factory | — | add-on module; MinPawns 4; may be absent |
| `Outpost_Encampment` | plain camp | — (the constraint-free fallback) | — |
| `Outpost_Artillery` | artillery emplacement | — | player-defense mechanic; seed as border dressing |
| `Outpost_Defensive` | garrison | — | player-defense mechanic; MinPawns 3 |

Artillery and Defensive are pure player-loop mechanics (they fire on threats to *your*
colony). As AI world-objects they are inert, so they are seeded **only** as frontier-garrison
dressing on contested edges, never in the interior.

## 2. The selection model — three axes

A tile resolves to an archetype through three axes applied in order. Terrain is a hard filter;
position and faction character are soft weights over what terrain allows.

### Axis A — terrain gate (hard, already exists)
The VOE hard gates above are absolute: a mine cannot sit on flat desert, drilling cannot sit
on a forest. This is what `OutpostArchetypeRules` already encodes via `TileFeatures`
(hilliness, plant/tree/animal density, minerals, coastal). It stays, and grows the desert case
(desert → Drilling instead of the current fall-through to Encampment).

### Axis B — position in the territory (new)
Distance from the tile to its province **anchor** — the highest-tier settlement of the
province, which the seeding pass already computes. Normalise 0 (at the capital) → 1 (province
edge) and bias the type:

- **Capital core** — civilian & economic, kept close and safe: Farming, Town, Trading,
  Science, Production, Factory.
- **Interior** — general holdings: Hunting, Encampment, Scavenging.
- **Frontier edge** — extraction pushed out, plus border garrisons: Mining, Logging, Drilling,
  Defensive, Artillery.

### Axis C — faction character (new)
Bias the mix by the faction's `def.techLevel` and hostility:

- **Tribal** (`techLevel <= Neolithic`): favour Hunting, Farming, Logging, Encampment; forbid
  Production, Science, Factory, Artillery (no tech base for them).
- **Industrial+**: unlock Production, Science, Factory, Trading, Town.
- **Hostile / hidden** (`permanentEnemy`, or a raider/pirate faction): favour Scavenging,
  Defensive, Mining; disfavour Town, Trading, Science.

### Resolution
`Choose` becomes a weighted scorer rather than a strict priority chain: for each archetype,
`score = terrainAllowed ? (positionWeight × factionWeight) : 0`; pick the argmax, breaking ties
deterministically (or a seeded weighted pick for variety). Falls back to Encampment when
nothing scores. Must stay **pure** — no `Find`, no Unity — so it remains unit-testable; the
seeding facade fills the inputs.

## 3. Where the work lives (cross-repo split)

`IHoldingCreator.TryCreate(kind, archetype, faction, tile, out created)` hands this patch only
the archetype, faction, and tile — **never** the province or the anchor settlement. The spatial
reasoning therefore cannot live here; it lives in core's seeding loop, which already has the
province anchor in hand.

- **Core-MMF** (companion issue): grow the `OutpostArchetype` enum to the full roster; add the
  position + faction inputs to `TileFeatures`; rewrite `OutpostArchetypeRules.Choose` as the
  weighted scorer; feed anchor-distance and faction character from `OutpostSeedingUtility`.
- **VOE-CP** (this repo, #3): extend `VoeOutpostCreator.DefName` to map every new archetype to
  its VOE def (table below), keep the graceful fallback chain (archetype def → Encampment →
  any `Outposts.Outpost` def), and honour MinPawns/module-absent cases (Factory add-on).

### Archetype → VOE def mapping (patch side)

```
Encampment  -> Outpost_Encampment      Trading    -> Outpost_Trading
Mining      -> Outpost_Mining          Production -> Outpost_Production
Logging     -> Outpost_Logging         Science    -> Outpost_Science
Farming     -> Outpost_Farming         Town       -> Outpost_Town
Hunting     -> Outpost_Hunting          Scavenging -> Outpost_Scavenging
Drilling    -> Outpost_Drilling        Factory    -> Outpost_Factory (add-on; fallback if absent)
Artillery   -> Outpost_Artillery       Defensive  -> Outpost_Defensive
```

## 4. Open tuning questions (decide in-game against the seeding debug report)

- Position band cutoffs (what distance fraction is "core" vs "frontier").
- Whether Town's "3 settlements within 10 hexes" gate is honoured at seed time or left to VOE.
- Faction-character source of truth: raw `techLevel` + `permanentEnemy`, or a richer VFE signal.
- Whether Artillery/Defensive seed at all for peaceful factions (probably not).

[#3]: https://github.com/Regions-and-societies/VOE-CP/issues/3
