using System.Globalization;
using System.Text;
using Pantheon.Core.Geometry;
using Pantheon.Core.Workframes;

namespace Pantheon.Demo;

/// <summary>
/// Writes a workframe as an AutoCAD R12 (AC1009) DXF file: lines, grid circles and grid labels on one
/// layer per line kind, coloured as the add-in colours them. Coordinates are inches.
/// </summary>
internal static class WorkframeDxf
{
    private const string Linetype = "CONTINUOUS";
    private const string GridLayer = "PANTHEON-GRID";
    private const int GridColor = 3;

    private static readonly IReadOnlyDictionary<LineKind, (string Name, int Color)> Layers =
        new Dictionary<LineKind, (string Name, int Color)>
        {
            [LineKind.Frame] = ("PANTHEON-FRAME", 3),
            [LineKind.Offset] = ("PANTHEON-PURLIN", 2),
            [LineKind.Base] = ("PANTHEON-FOOTPRINT", 20),
            [LineKind.Annotation] = (GridLayer, GridColor),
        };

    public static string Write(WorkframeGeometry geometry)
    {
        var dxf = new Writer();
        dxf.Section("HEADER");
        dxf.Pair(9, "$ACADVER").Pair(1, "AC1009");
        dxf.Pair(0, "ENDSEC");

        dxf.Section("TABLES");
        dxf.Pair(0, "TABLE").Pair(2, "LTYPE").Pair(70, 1);
        dxf.Pair(0, "LTYPE").Pair(2, Linetype).Pair(70, 0).Pair(3, "Solid line").Pair(72, 65).Pair(73, 0).Pair(40, 0.0);
        dxf.Pair(0, "ENDTAB");
        var layers = Layers.Values.Distinct().ToArray();
        dxf.Pair(0, "TABLE").Pair(2, "LAYER").Pair(70, layers.Length);
        foreach (var (name, color) in layers)
        {
            dxf.Pair(0, "LAYER").Pair(2, name).Pair(70, 0).Pair(62, color).Pair(6, Linetype);
        }

        dxf.Pair(0, "ENDTAB");
        dxf.Pair(0, "ENDSEC");

        dxf.Section("ENTITIES");
        foreach (var line in geometry.Lines)
        {
            dxf.Pair(0, "LINE").Pair(8, Layers[line.Kind].Name);
            dxf.Point(10, line.StartPoint).Point(11, line.EndPoint);
        }

        foreach (var bubble in geometry.Bubbles)
        {
            dxf.Pair(0, "CIRCLE").Pair(8, GridLayer).Point(10, bubble.Center).Pair(40, bubble.Radius);
            // Centre and middle justification place the label on the second alignment point.
            dxf.Pair(0, "TEXT").Pair(8, GridLayer).Point(10, bubble.Center).Pair(40, bubble.Radius * 0.8)
                .Pair(1, bubble.Label).Pair(72, 1).Pair(73, 2).Point(11, bubble.Center);
        }

        dxf.Pair(0, "ENDSEC");
        dxf.Pair(0, "EOF");
        return dxf.ToString();
    }

    private sealed class Writer
    {
        private readonly StringBuilder _text = new();

        public Writer Section(string name) => Pair(0, "SECTION").Pair(2, name);

        public Writer Pair(int code, string value)
        {
            _text.Append(code.ToString(CultureInfo.InvariantCulture)).Append("\r\n").Append(value).Append("\r\n");
            return this;
        }

        public Writer Pair(int code, int value) => Pair(code, value.ToString(CultureInfo.InvariantCulture));

        public Writer Pair(int code, double value) => Pair(code, value.ToString("0.0#####", CultureInfo.InvariantCulture));

        /// <summary>A point as the code's X, the code plus 10's Y and the code plus 20's Z.</summary>
        public Writer Point(int code, Point3 point) => Pair(code, point.X).Pair(code + 10, point.Y).Pair(code + 20, point.Z);

        public override string ToString() => _text.ToString();
    }
}
