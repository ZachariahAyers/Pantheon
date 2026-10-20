# Changelog

## v2.0.0 - 2026-10-19

AutoCAD 2027 add-in in VB.NET that draws metal building framing (workframes, slabs, base plates, girts and purlins) from an engineer's numbers.

### Added

- Targets AutoCAD 2027 and .NET 10, with SDK-style projects and a `.slnx` solution. AutoCAD references come from Autodesk's `AutoCAD.NET` package.
- The drawing calculations live in `Pantheon.Core`, a library with no AutoCAD reference, covered by xUnit tests. The add-in holds only commands, dialogs and drawing writes.
- Standard WinForms dialogs; the add-in builds without a paid licence.
- One Pantheon ribbon tab holds the commands that draw something.
- Dialogs are modal and the member is drawn after the dialog closes.
- Entities take the colour indexes the code asks for (ACI 20 footprint, 3 frames and grid, 2 offsets). Members go on their layer without changing the current layer.
- Shapes and members follow the current UCS, so the Pitch palette and the shape commands work together.
- Bays longer than the building are refused with a message. Grid letters after Z run AA, AB and on.
- The custom pitch angle is written with the invariant decimal mark, so AutoCAD reads it in every locale.
- The palette opens with `PANTHEONPALETTE`.
- Option Strict is on and the .NET recommended analyzers fail the build on any warning.
- CI builds and tests on every push to master; Dependabot checks dependencies monthly.
- A browser drawing desk runs the real `Pantheon.Core` workframe geometry through a small .NET WebAssembly adapter and renders its returned line endpoints at `/opensource/pantheon/`.
- The browser desk draws gable, single slope and off-center ridge workframes with their grid bubbles, writes lengths in feet and inches to the nearest sixteenth, and downloads the frame as a DXF drawing in inches.
- The ribbon tab appears when AutoCAD starts, and adds Zee, Cee, Eave Strut, Palettes and About buttons.
- `PANTHEONABOUT` shows the version and opens the quick start, the privacy policy, an email to support or the source code.
- A Release build lays out the Autodesk App Store bundle (`Pantheon.bundle` and `Pantheon.bundle.zip`) with its quick start page.
- A member on a locked layer is refused with a message before anything is drawn; one drawn on an off or frozen layer is reported. Any failure, including one from AutoCAD, is shown on the command line instead of an error dialog.
- One undo takes back a workframe and the view change that showed it.
- The add-in also compiles on Linux and macOS.
