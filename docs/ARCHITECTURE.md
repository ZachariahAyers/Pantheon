# Architecture

Pantheon is two libraries and a test project.

```
Pantheon.AutoCAD  ->  Pantheon.Core
       |
       v
  AutoCAD 2027 .NET API (AutoCAD.NET package, compile only)

Pantheon.Core.Tests  ->  Pantheon.Core
```

## Pantheon.Core

Every calculation that decides where a line goes. It targets `net10.0` and references nothing but the base library, so it builds and tests on any OS.

| Folder | Holds |
| --- | --- |
| `Geometry` | `Point2`, `Point3`, `Outline2` (an ordered polyline, open or closed) and the shared argument checks |
| `Roofs` | `RightTriangle` (run, rise and sloped length at a pitch) and `Pitch` (rise per 12, angles, the UCS command string) |
| `Profiles` | `CeeProfile`, `ZeeProfile`, `EaveStrutProfile`: section outlines |
| `Workframes` | The three building shapes, bay parsing, grid layout and the line set a workframe draws |
| `Members` | `Slab`, `BasePlate`, `AnchorRods` |

Models validate in their constructors and throw `ArgumentException` (or `FormatException` for bay text) with a message a drafter can act on. The add-in shows that message as it is.

### Coordinates

- Pitch is a ratio, rise over run. Dialogs take rise per 12 and convert with `Pitch.FromRisePerTwelve`.
- Section outlines are relative to the picked point: the web midpoint for a cee, the top of the web for a zee.
- Members are built about the origin. The add-in places them at the picked point in the current UCS.
- A workframe's origin is its bottom-left corner. X runs along the length, Y across the width, Z up. Frame lines sit at X positions; each frame is a profile in the Y-Z plane.

### Workframes

`SymmetricalWorkframe`, `SingleSlopeWorkframe` and `OffsetRidgeWorkframe` implement `IWorkframe.Build(textHeight)`, which returns a `WorkframeGeometry`: lines tagged by kind (base, frame, offset, annotation) and grid bubbles.

- `FrameProfile` is one frame seen from the end: a column at each side and the roof between. Its purlin line drops each roof segment by the purlin depth measured square to the roof (`depth * sqrt(1 + pitch^2)`) and joins neighbouring segments where their offset lines meet. With bypass girts, the purlin line starts and ends a girt depth inside each wall, on the roof slope.
- `GridLayout` turns bay sizes into grid positions: both ends plus every bay line between them. Bays that run past the building are refused.
- `WorkframeBuilder` drops exact repeats, so a line shared by two frames or two halves is drawn once.
- An off-center ridge building is two single slope frames back to back, each with its own width, length, eave, pitch and framing.

## Pantheon.AutoCAD

The add-in. It targets `net10.0-windows` with WinForms and WPF, and holds no geometry math.

| Folder | Holds |
| --- | --- |
| `Commands` | The `CommandMethod` entry points: interface, member and shape commands |
| `Drawing` | `DrawingTarget` (transactions, layers, blocks, UCS), `MemberDrawing` (Core model to entities), conversions |
| `Ui` | Ribbon tab, palettes, icons, and `Ui/Members`: the generic member dialog and the catalogue of member types |

A member command runs this flow:

1. `MemberDefinition.Run` shows a `MemberDialog` built from the member's field list.
2. On Create, `MemberValues` reads the entries (distances through AutoCAD's unit parser) and the definition builds the Core model. Any error keeps the dialog open with its message.
3. After the dialog closes, `MemberDrawing` asks for the insertion point and `DrawingTarget.InsertBlock` writes the block in one transaction.

The member types, their fields and their defaults are all in `Ui/Members/MemberCatalog.vb`.

## Tests

`tests/Pantheon.Core.Tests` uses xUnit v3. Expected vertices are worked by hand or from closed forms, never produced by the code under test.

CI builds the whole solution and runs these tests on a Windows runner. The add-in compiles there against Autodesk's reference assemblies, but nothing can run inside AutoCAD on a runner; drawing behaviour is checked by loading the add-in in AutoCAD 2027.
