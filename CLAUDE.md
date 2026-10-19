# Pantheon

AutoCAD 2027 add-in in VB.NET on .NET 10. It draws metal building framing (workframes, slabs, base plates, cold-formed sections) from an engineer's numbers.

## Commands

- Build: `dotnet build Pantheon.slnx -c Release`
- Test: `dotnet test Pantheon.slnx -c Release`
- Core tests alone (any OS): `dotnet test tests/Pantheon.Core.Tests`
- The add-in builds only on Windows (`net10.0-windows`). Nothing here runs AutoCAD: check a drawing change by NETLOAD of `src/Pantheon.AutoCAD/bin/Release/net10.0-windows/Pantheon.dll` in AutoCAD 2027.

## Architecture

- `src/Pantheon.Core`: all geometry. No AutoCAD, no UI. A new calculation goes here, with a test.
- `src/Pantheon.AutoCAD`: commands, ribbon, palettes, dialogs, database writes. No geometry math.
- `src/Pantheon.AutoCAD/Ui/Members/MemberCatalog.vb`: every member type's dialog fields and how they build a Core model.
- `tests/Pantheon.Core.Tests`: xUnit v3.
- Details and coordinate conventions: `docs/ARCHITECTURE.md`.

## Conventions

- Option Strict On and warnings as errors, including the .NET recommended analyzers. Fix the cause; do not suppress.
- VB is case-insensitive: never name a local after a type or module used in the same scope (`line` hides `Line`, `pitch` hides `Pitch`).
- Format numbers AutoCAD will read with `CultureInfo.InvariantCulture`; parse what a drafter types with `CurrentCulture`.
- Bad input throws `ArgumentException` with a message a drafter can act on. The add-in shows it unchanged.
- Check every AutoCAD API member against the `AutoCAD.NET` version in `Directory.Packages.props`. Never guess an API.
- Package versions live only in `Directory.Packages.props`.
- Test expectations come from hand-worked numbers or closed forms, never from the code under test.
- Plain comments: what the code does and why, never its history.

## Definition of done

- Build and tests pass with zero warnings.
- New or changed Core behaviour has a test.
- `CHANGELOG.md` notes anything a drafter would notice; `README.md` stays user-facing.
