// Program.cs  (interactive test program for the adjacency list Graph<T> of section 5.3)
using System;
using System.Linq;
using Backend;

Console.Write("Directed graph? (y/n): ");
bool directed = (Console.ReadLine()?.Trim().ToLowerInvariant() ?? "") is "y" or "yes";
var graph = new Graph<string>(directed);

while (true)
{
    Console.WriteLine();
    Console.WriteLine($"=== {(graph.IsDirected ? "Directed" : "Undirected")} graph: {graph.VertexCount} vertices, {graph.EdgeCount} edges ===");
    Console.WriteLine("1. Add a vertex");
    Console.WriteLine("2. Add an edge (with optional weight; creates the vertices if needed)");
    Console.WriteLine("3. Remove a vertex (and every edge that touches it)");
    Console.WriteLine("4. Remove an edge");
    Console.WriteLine("5. Show the graph (adjacency lists, drawing and tree view)");
    Console.WriteLine("6. Check whether an edge exists");
    Console.WriteLine("7. Neighbors and degree of a vertex");
    Console.WriteLine("8. List every edge once");
    Console.WriteLine("9. Clear the whole graph (start from scratch)");
    Console.WriteLine("0. Exit");
    Console.Write("Option: ");

    string option = Console.ReadLine()?.Trim() ?? "";

    switch (option)
    {
        case "1":
            {
                string? vertex = ReadValue("Vertex: ");
                if (vertex == null) break;

                Console.WriteLine(graph.AddVertex(vertex)
                    ? $"  \"{vertex}\" added (isolated, no edges yet)."
                    : $"  \"{vertex}\" is already in the graph.");
                break;
            }
        case "2":
            {
                string? from = ReadValue("From: ");
                if (from == null) break;
                string? to = ReadValue("To: ");
                if (to == null) break;

                Console.Write("Weight (blank = 1): ");
                string weightText = Console.ReadLine()?.Trim() ?? "";
                double weight = 1;
                if (weightText.Length > 0 && !double.TryParse(weightText, out weight))
                {
                    Console.WriteLine($"  \"{weightText}\" is not a valid number.");
                    break;
                }

                if (graph.HasEdge(from, to))
                {
                    Console.WriteLine($"  The edge {from} -> {to} already exists (no parallel edges).");
                    break;
                }

                int verticesBefore = graph.VertexCount;
                graph.AddEdge(from, to, weight);
                Console.WriteLine($"  Edge {from} {(graph.IsDirected ? "->" : "--")} {to} added with weight {weight}.");
                if (graph.VertexCount > verticesBefore)
                {
                    Console.WriteLine($"  ({graph.VertexCount - verticesBefore} new vertex/vertices were created on the fly.)");
                }
                break;
            }
        case "3":
            {
                string? vertex = ReadExistingVertex("Vertex to remove: ");
                if (vertex == null) break;

                int edgesBefore = graph.EdgeCount;
                graph.RemoveVertex(vertex);
                Console.WriteLine($"  \"{vertex}\" removed together with {edgesBefore - graph.EdgeCount} edge(s).");
                break;
            }
        case "4":
            {
                string? from = ReadExistingVertex("From: ");
                if (from == null) break;
                string? to = ReadExistingVertex("To: ");
                if (to == null) break;

                Console.WriteLine(graph.RemoveEdge(from, to)
                    ? $"  Edge {from} -> {to} removed."
                    : $"  There is no edge {from} -> {to}.");
                break;
            }
        case "5":
            {
                if (graph.VertexCount == 0) { Console.WriteLine("  The graph is empty."); break; }

                graph.Print();
                Console.WriteLine();
                GraphPrinter.PrintCircular(graph);

                Console.Write($"Start vertex for the tree view (blank = {graph.Vertices.First()}): ");
                string start = Console.ReadLine()?.Trim() ?? "";
                if (start.Length == 0) start = graph.Vertices.First();
                if (!graph.ContainsVertex(start)) { Console.WriteLine($"  \"{start}\" is not in the graph."); break; }

                Console.WriteLine();
                GraphPrinter.PrintAsTree(graph, start);
                break;
            }
        case "6":
            {
                string? from = ReadExistingVertex("From: ");
                if (from == null) break;
                string? to = ReadExistingVertex("To: ");
                if (to == null) break;

                if (graph.HasEdge(from, to))
                {
                    var edge = graph.EdgesFrom(from).First(e => e.To == to);
                    Console.WriteLine($"  Edge {from} -> {to} EXISTS with weight {edge.Weight}.");
                }
                else
                {
                    Console.WriteLine($"  Edge {from} -> {to} does NOT exist.");
                }
                break;
            }
        case "7":
            {
                string? vertex = ReadExistingVertex("Vertex: ");
                if (vertex == null) break;

                var neighbors = graph.Neighbors(vertex).ToList();
                Console.WriteLine(neighbors.Count == 0
                    ? $"  {vertex} has no neighbors."
                    : $"  Neighbors of {vertex}: {string.Join(" ", neighbors)}");
                Console.WriteLine(graph.IsDirected
                    ? $"  Out-degree: {graph.Degree(vertex)}, in-degree: {graph.InDegree(vertex)}"
                    : $"  Degree: {graph.Degree(vertex)}");
                break;
            }
        case "8":
            {
                var edges = graph.Edges().ToList();
                if (edges.Count == 0) { Console.WriteLine("  The graph has no edges."); break; }

                Console.WriteLine($"  {edges.Count} edge(s):");
                foreach (var edge in edges) Console.WriteLine($"    {edge}");
                break;
            }
        case "9":
            {
                if (graph.VertexCount == 0) { Console.WriteLine("  The graph is already empty."); break; }

                Console.Write($"  This will delete {graph.VertexCount} vertices and {graph.EdgeCount} edges. Are you sure? (y/n): ");
                if ((Console.ReadLine()?.Trim().ToLowerInvariant() ?? "") is "y" or "yes")
                {
                    graph = new Graph<string>(graph.IsDirected); // A fresh graph of the same kind.
                    Console.WriteLine("  The graph was cleared: you can start from scratch.");
                }
                else
                {
                    Console.WriteLine("  Cancelled, the graph was not modified.");
                }
                break;
            }
        case "0":
            Console.WriteLine("Bye.");
            return;
        default:
            Console.WriteLine("  Invalid option. Choose 0 to 9.");
            break;
    }
}

// Reads a non-empty text value; returns null (and prints a message) if it is blank.
static string? ReadValue(string prompt)
{
    Console.Write(prompt);
    string input = Console.ReadLine()?.Trim() ?? "";

    if (input.Length > 0) return input;

    Console.WriteLine("  The value cannot be blank.");
    return null;
}

// Like ReadValue, but the vertex must already exist in the graph.
string? ReadExistingVertex(string prompt)
{
    string? vertex = ReadValue(prompt);
    if (vertex == null) return null;

    if (graph.ContainsVertex(vertex)) return vertex;

    Console.WriteLine($"  \"{vertex}\" is not in the graph.");
    return null;
}
