# Regions and Societies: Outposts Expanded Patch

Welcome to the documentation for the **Vanilla Outposts Expanded compatibility patch** for [Regions and Societies](https://github.com/Regions-and-societies/Core-MMF). This is a companion patch mod (`RegionsAndSocieties.VOECP`): install it alongside Regions and Societies and Vanilla Outposts Expanded, and VOE outposts are classified, populated and owned under territory rules; world generation seeds VOE outposts around settlements; and outpost placement respects regional ownership.

## Table of Contents

- [Player's Guide](Players_Guide) — what the patch does in your game
- [Developer's Guide](Developers_Guide) — the patch's structure and the core API it binds

---

## Before you start

- **Requires both parents.** [Regions and Societies](https://steamcommunity.com/sharedfiles/filedetails/?id=3784666060) and [Vanilla Outposts Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=2688941031) must both be installed; the patch loads after both.
- **Installed means enabled.** The patch has no settings of its own. The only relevant toggle is core's master governance switch, which lives in the Regions and Societies settings.
- **Load order** is declared in About.xml: after Regions and Societies core, the Vanilla Expanded Framework and Vanilla Outposts Expanded. RimSort/RimPy and the game's own sorter handle it automatically.

Source: [Regions-and-societies/VOE-CP](https://github.com/Regions-and-societies/VOE-CP) · Licence: PolyForm Noncommercial 1.0.0 · Support development: [ko-fi.com/archdukejim](https://ko-fi.com/archdukejim)
