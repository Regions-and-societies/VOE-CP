# Developer's Guide

How the Outposts Expanded patch is built, and the surfaces it exposes. This patch is primarily a **consumer** of the core extension points documented in the [core Developer's Guide](https://github.com/Regions-and-societies/Core-MMF/wiki/Developers_Guide) — it is also the reference implementation of a typed companion patch, so its three pieces are documented here the same way.

The assembly is `RegionsAndSocieties.VOECP`, namespace `RegionsAndSocieties.VOECP`. It references the `Outposts` assembly directly (under RimWorld 1.6 that assembly ships inside the Vanilla Expanded Framework; VOE supplies the concrete outposts and defs), so every binding is a compile-time fact rather than a reflection lookup.

Contents:

- [Mod entry and wiring](#mod-entry-and-wiring)
- [VoeAdapter — the read side](#voeadapter--the-read-side)
- [VoeOutpostCreator — the write side](#voeoutpostcreator--the-write-side)
- [Placement-gate Harmony patch](#placement-gate-harmony-patch)
- [Debug action](#debug-action)

---

## Mod entry and wiring

`VOECPMod`'s constructor is the entire integration:

```csharp
var harmony = new Harmony("regionsandsocieties.voecp");
harmony.PatchAll();

WorldObjectAdapterRegistry.Register(new VoeAdapter());
HoldingCreatorRegistry.Register(new VoeOutpostCreator());
```

The patch loads after core (declared `loadAfter`), so both registries are live when registration runs. Startup logs one line: `[RegionsAndSocieties.VOECP] Registered the Vanilla Outposts Expanded adapter and outpost creator (priority 110).`

## VoeAdapter — the read side

`public class VoeAdapter : WorldObjectAdapterBase` — teaches core to *read* VOE outposts.

| Member | Value / behaviour |
|---|---|
| `AdapterId` | `"voe"` |
| `DisplayName` | `"Vanilla Outposts Expanded"` |
| `Priority` | `110` (between Empire at 100 and VFE at 120 — the slot core's old reflection profile held) |
| `IsActive` | `ModsConfig.IsActive("vanillaexpanded.outposts")`, evaluated once |

### TryClassify

```csharp
public override bool TryClassify(WorldObject obj, out WorldObjectKind kind)
```

Returns `WorldObjectKind.Outpost` for any `Outposts.Outpost` subclass (covers every concrete VOE outpost), with a namespace fallback (`Namespace == "Outposts"`) preserving the old reflection profile's behaviour for any future non-`Outpost` type the mod introduces.

### TryGetPopulation

```csharp
public override bool TryGetPopulation(WorldObject obj, out int population)
```

Reports `Outpost.PawnCount` — the real occupant count — so VOE outposts weigh into ownership and demographics instead of reading zero. Returns `false` for anything that is not an `Outposts.Outpost`.

## VoeOutpostCreator — the write side

`public class VoeOutpostCreator : IHoldingCreator` — teaches core to *build* VOE outposts during worldgen seeding.

| Member | Value / behaviour |
|---|---|
| `CreatorId` | `"voe"` |
| `Priority` | `110` (matches the adapter, so read and write sides act as one integration) |
| `IsActive` | Core's `WorldObjectIntegrationSettings.masterEnabled` — installed means enabled |
| `CanCreate` | `kind == WorldObjectKind.Outpost` |

### TryCreate

```csharp
public bool TryCreate(WorldObjectKind kind, OutpostArchetype archetype,
                      Faction faction, int tile, out WorldObject created)
```

The creation recipe, established against the decompiled 1.6 `Outposts.dll`:

1. **Resolve the def** from the archetype: `Outpost_Mining` / `Outpost_Logging` / `Outpost_Farming` / `Outpost_Hunting`, falling back to the constraint-free `Outpost_Encampment`, then to *any* `WorldObjectDef` whose class derives from `Outposts.Outpost` — a renamed or reduced VOE def set degrades gracefully instead of failing hard.
2. `WorldObjectMaker.MakeWorldObject(def)` → set `Tile` → `SetFaction(faction)` (must be non-null or VOE's world-map material getter throws) → set the public `Name` field (generated from the faction's own `settlementNameMaker`, falling back to `"<faction> outpost"`) → `Find.WorldObjects.Add`.
3. **Attach one occupant** via the outpost's public `AddPawn(Pawn)` — a generated pawn of the faction's `basicMemberKind`. This is not optional: a VOE outpost that ends a tick with no occupants deletes itself. If `AddPawn` fails, the outpost is destroyed cleanly and `TryCreate` returns `false`.

VOE's own `CanSpawnOnWithExt` is deliberately **not** consulted during seeding: it validates a would-be caravan's pawn skills and headcount, not the tile, and would reject every seeding for having no caravan. Archetype selection already matches terrain.

Every failure path logs an `ErrorOnce` with a distinct key (`0x5B0101`–`0x5B0105`) and returns `false`, so core falls through to other creators or skips the site.

## Placement-gate Harmony patch

`Patch_Outposts_CanSpawnOn` — a postfix on `Outposts.Utils.CanSpawnOnWithExt`. When core's `OutpostPlacementUtility.CanPlaceOutpostAt` refuses a tile for the player faction, the postfix writes core's reason string into VOE's result, so VOE's own establishment UI shows the refusal verbatim. If the VOE build lacks that method, `Prepare()` logs a warning and the patch is skipped rather than failing the load.

**Maintenance note:** the postfix's third parameter must be named `ps`, not `pawns` — Harmony binds injected parameters *by name* against the original signature, and VOE names it `ps`. The mismatch only manifests with VOE actually installed (`Parameter "pawns" not found`). Do not "tidy" the name.

## Debug action

Dev mode → Debug actions → *Regions and Societies* → **R&S VOE-CP: world-object dump**: logs every `Outposts.Outpost` on the world map with its resolved kind, population, tile and faction — including any foreign object that resolves through this adapter — so the classification path is observable in a running game.
