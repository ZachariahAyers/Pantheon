# Pantheon

[![CI](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml/badge.svg)](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml)

A free AutoCAD add-in that lets steel building drafters draw framing members straight from the engineer's numbers.

[Try a workframe in your browser](https://ayers.repair/opensource/pantheon/). The demo runs the real Pantheon.Core .NET library in WebAssembly and downloads the frame as a DXF drawing.

## Features

- Draws a building's 3D workframe (gable, single slope or off-center ridge) with frame lines, purlin and girt offsets, and grid bubbles.
- Draws foundation slabs, with an optional brick-ledge notch.
- Draws straight column base plates with anchor holes.
- Draws zee girts and purlins, cee jambs and posts, and eave struts at any roof pitch.
- Reads bay lists as engineers write them, such as `3*25',20'`.
- Places each member as a named block on its own layer.

## Tech stack

VB.NET, .NET 10, AutoCAD 2027 .NET API, WinForms, xUnit.

## Install

Pantheon is coming free to the Autodesk App Store, for AutoCAD 2027 on 64-bit Windows. Once installed, the Pantheon tab appears on the ribbon when AutoCAD starts.

## Development

Requires the .NET 10 SDK. The add-in compiles on any OS; only AutoCAD 2027 on Windows runs it.

```bash
dotnet build Pantheon.slnx -c Release
dotnet test Pantheon.slnx -c Release
node --test tests/*.test.mjs
```

To try a change in AutoCAD, run `NETLOAD` and pick `src/Pantheon.AutoCAD/bin/Release/net10.0-windows/Pantheon.dll`. A Release build also writes `Pantheon.bundle.zip`, the App Store package.

How it is put together: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).

---

Built by [Zachariah Ayers](https://ayers.repair).
