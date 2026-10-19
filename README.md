# Pantheon

[![CI](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml/badge.svg)](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml)

An AutoCAD add-in that draws metal building framing straight from the engineer's numbers.

By Zachariah Ayers. [ayers.repair](https://ayers.repair) · [GitHub](https://github.com/ZachariahAyers)

## The problem it solves

At a metal building fabricator, drafters turn engineering sheets into shop drawings. Most of that work is the same members drawn again and again: the frame lines of a building, its purlins and girts, the slab under it, the base plate under each column, the cold-formed sections in a detail.

Pantheon draws those members from the numbers on the sheet. The drafter types the width, eave height, pitch and bay sizes into a dialog. A few choices shape the parts: flush or bypass girts, a notch for a brick ledge, which anchor rod. Pantheon places the member as a named block on its own layer. The drafter finishes the details.

I built it at Cowell Steel Structures between 2013 and 2015. Version 2 rebuilds it for current AutoCAD.

## What it draws

| Command | Draws | From |
| --- | --- | --- |
| `WORKFRAME` | A 3D wireframe of the building: footprint, every frame line, the purlin and girt offset lines, and numbered and lettered grid bubbles. Symmetrical (gable), single slope or off-center ridge. | Width, eave heights, length, pitch, purlin and girt depths, flush or bypass girts, sidewall and endwall bays |
| `SLAB` | A foundation slab outline, with an optional hidden-line notch around it. | Width, length, notch depth |
| `COLUMN` | A straight column base plate as a 3D solid with four anchor holes. | Web depth, flange thicknesses, plate size, anchor rod, anchor spacing |
| `ZEE` | A zee girt or purlin section, purlins at any pitch. | Entry point, hand, web, flange, pitch |
| `CEE` | A cee jamb or post section. | Mid point, direction, web, flange |
| `EAVECEE` | An eave strut, double or single slope. | Web bottom, web top and a point on the roof line |

Bays are written the way engineers write them: `3*25',20'` is three 25 ft bays and one 20 ft bay.

## How it works in AutoCAD

1. `NETLOAD` the add-in, then run `PANTHEON`. It adds a Pantheon ribbon tab and opens the Pantheon palettes.
2. The ribbon starts the member commands. Each one opens a dialog for the member's numbers.
3. Give the member a mark. The mark becomes the block name.
4. Pick the insertion point. The member is drawn relative to the current UCS.

Pantheon remembers the members made in the session. Pick an earlier mark in the dialog to recall its numbers, or type a new mark over them to copy it.

The Shapes palette draws a zee, cee or eave strut in one click. The Pitch palette turns the UCS to a roof pitch (1:12 to 6:12, up or down, or any custom pitch), so sections can be drawn along the roof line and set back to World after.

Layers are created on first use: `WorkFrames`, `SLAB` and `Plates`. Grid bubbles are sized from the drawing's dimension text height (`DIMTXT`).

## Requirements

- AutoCAD 2027 on 64-bit Windows. Pantheon is built against the AutoCAD 2027 .NET API, which runs on .NET 10. Autodesk does not keep binary compatibility between 2025, 2026 and 2027, so another release needs a rebuild against its own API package.
- To build: the .NET 10 SDK on Windows. Visual Studio 2026 works; Autodesk notes that Visual Studio 2022 cannot target AutoCAD 2027.

## Build

```
dotnet build Pantheon.slnx -c Release
dotnet test Pantheon.slnx -c Release
```

The AutoCAD references come from Autodesk's `AutoCAD.NET` NuGet package, so no ObjectARX install is needed to compile. The add-in lands in `src/Pantheon.AutoCAD/bin/Release/net10.0-windows/`: `Pantheon.dll` and `Pantheon.Core.dll`.

Every push to master builds and tests on GitHub Actions and keeps the add-in as a build artifact.

## Load

1. In AutoCAD, run `NETLOAD` and pick `Pantheon.dll`. Keep `Pantheon.Core.dll` in the same folder.
2. Run `PANTHEON`.

## Project structure

```
Pantheon.slnx
src/
  Pantheon.Core/          Geometry with no AutoCAD reference: roof math, sections, workframes, slab, base plate
  Pantheon.AutoCAD/       The add-in: commands, ribbon, palettes, dialogs and the drawing writes
tests/
  Pantheon.Core.Tests/    xUnit tests for every calculation in Pantheon.Core
docs/
  ARCHITECTURE.md         How the pieces fit and the conventions the geometry uses
```

The drawing logic lives in `Pantheon.Core` so it can be tested without AutoCAD. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) and [CHANGELOG.md](CHANGELOG.md).

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).
