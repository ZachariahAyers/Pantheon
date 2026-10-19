# Changelog

## v2.0.0 - 2026-10-19

Built for current AutoCAD.

### Changed

- Targets AutoCAD 2027 and .NET 10, with SDK-style projects and a `.slnx` solution. AutoCAD references come from Autodesk's `AutoCAD.NET` package instead of an ObjectARX install path.
- The drawing calculations moved into `Pantheon.Core`, a library with no AutoCAD reference, covered by xUnit tests. The add-in keeps only commands, dialogs and drawing writes.
- The dialogs use standard WinForms. The Telerik 2014 controls are gone, so the add-in builds without a paid licence.
- One Pantheon ribbon tab holds the commands that draw something.
- Dialogs are modal and the member is drawn after the dialog closes.
- Entities take the colour indexes the code asked for (ACI 20 footprint, 3 frames and grid, 2 offsets). Members go on their layer without changing the current layer.
- Shapes and members follow the current UCS, so the Pitch palette and the shape commands work together.
- Option Strict is on and the .NET recommended analyzers fail the build on any warning.
- CI builds and tests on every push to master; Dependabot checks dependencies monthly.

### Fixed

- A right-hand purlin on a flat roof drew as a left-hand one.
- A pitched purlin's lips stayed at 45 degrees to the world instead of turning with the section.
- With bypass girts, the purlin line at the high eave of a single slope or off-center frame sat above the roof.
- The far half of an off-center ridge building was drawn from the wrong edge, with a diagonal base line.
- A bay list that filled the building drew a second frame and a doubled grid bubble at the back wall.
- Bays longer than the building were drawn past it. They are now refused with a message.
- Grid letters after Z repeated A. They now run AA, AB and on.
- Creating a member under a mark recalled from earlier in the session drew the earlier member's numbers instead of the ones just entered.
- The base plate was drawn at the drawing origin instead of the picked point, its anchor holes used the rod diameter as their radius, and empty cutter solids were left in the block.
- The eave strut `Bottomflange` option set the top flange.
- An eave strut pitch point that disagreed with the chosen style drew an empty polyline; a level pitch point drew nothing.
- The custom pitch angle was written with the current culture's decimal mark, which AutoCAD misread in some locales.
- `PANTHEONPALETT` is now `PANTHEONPALETTE`.

### Removed

- Ribbon buttons, tabs and palettes that ran no command: the Information, Generate, Tools and Options tabs, Save and Load Pantheon, and the Steel, Symbols, Angles, Plates, Panels, Trim and Fasteners palettes.
- `WALL` and the column types other than straight, which opened a form or prompt and drew nothing.
- The `Gauge` prompt option and the column's eave and pitch prompts, which never changed the drawing.
- The unused W-shape table, template files, editor leftovers and images nothing loaded.
