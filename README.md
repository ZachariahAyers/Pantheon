# Pantheon

[![CI](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml/badge.svg)](https://github.com/ZachariahAyers/Pantheon/actions/workflows/ci.yml)

An AutoCAD add-in that lets steel building drafters draw framing members straight from the engineer's numbers.

[Try the workframe geometry in your browser](https://ayers.repair/opensource/pantheon/). The demo runs the real Pantheon.Core .NET library in WebAssembly and produces no drawing file.

## Features

- Draws a building's 3D workframe (gable, single slope or off-center ridge) with frame lines, purlin and girt offsets, and grid bubbles.
- Draws foundation slabs, with an optional brick-ledge notch.
- Draws straight column base plates with anchor holes.
- Draws zee girts and purlins, cee jambs and posts, and eave struts at any roof pitch.
- Reads bay lists as engineers write them, such as `3*25',20'`.
- Places each member as a named block on its own layer.

## Tech stack

VB.NET, .NET 10, AutoCAD 2027 .NET API, WinForms, xUnit.

## Getting started

Requires AutoCAD 2027 and the .NET 10 SDK on Windows.

```bash
git clone https://github.com/ZachariahAyers/Pantheon.git
cd Pantheon
dotnet build Pantheon.slnx -c Release
```

In AutoCAD, run `NETLOAD` and pick `src/Pantheon.AutoCAD/bin/Release/net10.0-windows/Pantheon.dll`, then run `PANTHEON`.

## Development

```bash
dotnet build Pantheon.slnx -c Release
dotnet test Pantheon.slnx -c Release
```

How it is put together: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).

---

Built by [Zachariah Ayers](https://ayers.repair).
