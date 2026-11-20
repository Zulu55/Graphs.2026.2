// Backend/AdjacencyMatrixGraph.cs
namespace Backend;

/// <summary>
/// Graph stored as an adjacency matrix. Vertices are numbered 0..n-1 and the matrix
/// holds the edge weight, or double.PositiveInfinity when there is no edge.
/// </summary>
public class AdjacencyMatrixGraph
{
    private readonly double[,] _matrix;

    public string[] Labels { get; }
    public bool IsDirected { get; }
    public int VertexCount => Labels.Length;

    public AdjacencyMatrixGraph(string[] labels, bool directed = false)
    {
        Labels = labels;
        IsDirected = directed;
        _matrix = new double[labels.Length, labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            for (int j = 0; j < labels.Length; j++)
            {
                _matrix[i, j] = double.PositiveInfinity; // No edge yet.
            }
        }
    }

    public int IndexOf(string label)
    {
        int index = Array.IndexOf(Labels, label);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Vertex '{label}' is not in the graph.");
        }
        return index;
    }

    public void AddEdge(string from, string to, double weight = 1)
    {
        int i = IndexOf(from);
        int j = IndexOf(to);
        _matrix[i, j] = weight;
        if (!IsDirected)
        {
            _matrix[j, i] = weight;
        }
    }

    public void RemoveEdge(string from, string to)
    {
        int i = IndexOf(from);
        int j = IndexOf(to);
        _matrix[i, j] = double.PositiveInfinity;
        if (!IsDirected)
        {
            _matrix[j, i] = double.PositiveInfinity;
        }
    }

    /// <summary>O(1): the main advantage of the matrix.</summary>
    public bool HasEdge(string from, string to) => !double.IsPositiveInfinity(_matrix[IndexOf(from), IndexOf(to)]);

    public double Weight(string from, string to) => _matrix[IndexOf(from), IndexOf(to)];

    /// <summary>O(V): we must scan the whole row, even if the vertex has one neighbor.</summary>
    public IEnumerable<string> Neighbors(string vertex)
    {
        int i = IndexOf(vertex);
        for (int j = 0; j < VertexCount; j++)
        {
            if (!double.IsPositiveInfinity(_matrix[i, j]))
            {
                yield return Labels[j];
            }
        }
    }

    public void Print()
    {
        Console.Write("     ");
        foreach (string label in Labels) Console.Write($"{label,4}");
        Console.WriteLine();

        for (int i = 0; i < VertexCount; i++)
        {
            Console.Write($"{Labels[i],4} ");
            for (int j = 0; j < VertexCount; j++)
            {
                string cell = double.IsPositiveInfinity(_matrix[i, j]) ? "." : _matrix[i, j].ToString();
                Console.Write($"{cell,4}");
            }
            Console.WriteLine();
        }
    }
}
