// Backend/GraphPrinter.cs
namespace Backend;

/// <summary>
/// Draws a graph on the console in two ways: as an indented tree grown from one vertex,
/// and as a picture with the vertices on a circle and the edges as lines.
/// </summary>
public static class GraphPrinter
{
    // ---------- 1. Tree view: DFS from a start vertex with indentation ----------

    /// <summary>
    /// Prints the graph as a tree rooted at start. Every vertex is expanded the first time it
    /// appears; later appearances are printed but marked, which is how cycles and shared
    /// vertices show up. Vertices not reachable from start are listed at the end.
    /// </summary>
    public static void PrintAsTree<T>(Graph<T> graph, T start) where T : notnull
    {
        var shown = new HashSet<T>();
        PrintTreeNode(graph, start, parent: default, via: null, prefix: "", isLast: true, shown);

        var unreachable = graph.Vertices.Where(v => !shown.Contains(v)).ToList();
        if (unreachable.Count > 0)
        {
            Console.WriteLine($"(not reachable from {start}: {string.Join(", ", unreachable)})");
        }
    }

    private static void PrintTreeNode<T>(Graph<T> graph, T node, T? parent, Edge<T>? via,
                                         string prefix, bool isLast, HashSet<T> shown) where T : notnull
    {
        bool repeated = !shown.Add(node);
        string connector = via == null ? "" : (isLast ? "+-- " : "|-- ");
        string weight = via != null && via.Value.Weight != 1 ? $" ({via.Value.Weight})" : "";
        Console.WriteLine($"{prefix}{connector}{node}{weight}{(repeated ? "  *" : "")}");

        if (repeated) return; // Already expanded above: do not follow its edges again.

        // In an undirected graph the edge back to the parent is the one we came by: skip it.
        var edges = graph.EdgesFrom(node)
                         .Where(e => graph.IsDirected || parent == null || !e.To.Equals(parent))
                         .ToList();

        string childPrefix = via == null ? "" : prefix + (isLast ? "    " : "|   ");
        for (int i = 0; i < edges.Count; i++)
        {
            PrintTreeNode(graph, edges[i].To, node, edges[i], childPrefix, i == edges.Count - 1, shown);
        }
    }

    // ---------- 2. Circular view: vertices on a circle, edges as lines ----------

    /// <summary>
    /// Draws the vertices evenly spaced on a circle and every edge as a straight line between them.
    /// Directed edges get an arrowhead next to the target; weights other than 1 are written at
    /// the middle of the line. Meant for small graphs (up to about 10 vertices).
    /// </summary>
    public static void PrintCircular<T>(Graph<T> graph, int radius = 6) where T : notnull
    {
        var vertices = graph.Vertices.ToList();
        if (vertices.Count == 0)
        {
            Console.WriteLine("(empty graph)");
            return;
        }

        // Console characters are about twice as tall as wide: stretch x so the circle looks round.
        int labelWidth = vertices.Max(v => v.ToString()!.Length);
        int rx = radius * 2 + labelWidth, ry = radius;
        int width = rx * 2 + labelWidth * 2 + 2, height = ry * 2 + 1;
        int cx = width / 2, cy = ry;

        var canvas = new char[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                canvas[y, x] = ' ';

        // Position of every vertex on the ellipse, starting at the top and going clockwise.
        var position = new Dictionary<T, (int X, int Y)>();
        for (int i = 0; i < vertices.Count; i++)
        {
            double angle = 2 * Math.PI * i / vertices.Count - Math.PI / 2;
            position[vertices[i]] = (cx + (int)Math.Round(rx * Math.Cos(angle)),
                                     cy + (int)Math.Round(ry * Math.Sin(angle)));
        }

        // Edges first, so the labels drawn afterwards stay on top.
        var edges = graph.Edges().ToList();
        foreach (var edge in edges)
        {
            var (x0, y0) = position[edge.From];
            var (x1, y1) = position[edge.To];
            DrawLine(canvas, x0, y0, x1, y1);
        }

        // Arrowheads and weights in a second pass, so no line drawn later covers them.
        foreach (var edge in edges)
        {
            var (x0, y0) = position[edge.From];
            var (x1, y1) = position[edge.To];
            if (graph.IsDirected) DrawArrowhead(canvas, x0, y0, x1, y1);
            if (edge.Weight != 1) WriteText(canvas, (x0 + x1) / 2, (y0 + y1) / 2, edge.Weight.ToString());
        }

        foreach (var (vertex, (x, y)) in position)
        {
            string label = vertex.ToString()!;
            WriteText(canvas, x, y, label); // Centered on the vertex position.
        }

        for (int y = 0; y < height; y++)
        {
            var row = new char[width];
            for (int x = 0; x < width; x++) row[x] = canvas[y, x];
            Console.WriteLine(new string(row).TrimEnd()); // Every row, even blank ones: the drawing needs them.
        }
    }

    /// <summary>The cells of a straight line between two points (Bresenham's algorithm).</summary>
    private static List<(int X, int Y)> LineCells(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;

        int x = x0, y = y0, err = dx - dy;
        var cells = new List<(int X, int Y)>();
        while (true)
        {
            cells.Add((x, y));
            if (x == x1 && y == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
        return cells;
    }

    /// <summary>Draws the line with a character chosen by its slope; crossings become '+'.</summary>
    private static void DrawLine(char[,] canvas, int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        bool downRight = (x1 > x0) == (y1 > y0);
        char stroke = dy == 0 || dx >= 3 * dy ? '-'      // Horizontal or nearly so.
                    : dx == 0 || 2 * dx <= dy ? '|'      // Vertical or nearly so.
                    : downRight ? '\\' : '/';            // Diagonal.

        var cells = LineCells(x0, y0, x1, y1);
        for (int i = 1; i < cells.Count - 1; i++) // Skip the ends: they belong to the vertex labels.
        {
            var (x, y) = cells[i];
            char current = canvas[y, x];
            canvas[y, x] = current == ' ' || current == stroke ? stroke : '+';
        }
    }

    /// <summary>Puts an arrowhead on the cell right before the target label, pointing at it.</summary>
    private static void DrawArrowhead(char[,] canvas, int x0, int y0, int x1, int y1)
    {
        var cells = LineCells(x0, y0, x1, y1);
        if (cells.Count < 3) return;

        var (x, y) = cells[^2];
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        canvas[y, x] = dx >= dy ? (x1 > x0 ? '>' : '<') : (y1 > y0 ? 'v' : '^');
    }

    private static void WriteText(char[,] canvas, int centerX, int centerY, string text)
    {
        int width = canvas.GetLength(1);
        int startX = Math.Clamp(centerX - text.Length / 2, 0, Math.Max(0, width - text.Length));
        for (int i = 0; i < text.Length; i++) canvas[centerY, startX + i] = text[i];
    }
}
