using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HitoGrafosAsignacion;

public static class Program
{
    // Escenarios: 20 trabajadores (base del proyecto) y ampliaciones hipoteticas.
    private static readonly int[] WorkerSizes = { 20, 100, 500, 1000 };
    private const int EdgesPerWorker = 4;
    private const int MeasuredRuns = 3;

    private sealed record GraphData(List<int>[] List, bool[,] Matrix, int Workers, int Posts, int EdgeCount);

    public static void Main()
    {
        Console.WriteLine("HITO GRAFOS - Asignacion de personal a proyectos electricos");
        Console.WriteLine("BFS/DFS: lista de adyacencia frente a matriz de adyacencia");
        Console.WriteLine("Datos sinteticos. Los escenarios mayores son proyecciones hipoteticas.\n");

        var detail = new List<string>
        {
            "Representacion,Algoritmo,Trabajadores,Puestos,Vertices,Aristas,Repeticion,Tiempo_ms,NodosVisitados"
        };
        var summaries = new List<Summary>();

        foreach (int n in WorkerSizes)
        {
            GraphData graph = BuildGraph(n);
            int vertices = graph.Workers + graph.Posts;

            // Calentamiento por cada combinacion; no se incorpora a las mediciones.
            foreach (string representation in new[] { "Lista", "Matriz" })
            foreach (string algorithm in new[] { "BFS", "DFS" })
            {
                int warm = Run(graph, representation, algorithm);
                if (warm != vertices)
                    throw new InvalidOperationException("El recorrido no visito todos los nodos del bosque.");
            }

            foreach (string representation in new[] { "Lista", "Matriz" })
            foreach (string algorithm in new[] { "BFS", "DFS" })
            {
                var times = new List<double>();
                int expectedVisited = -1;

                for (int run = 1; run <= MeasuredRuns; run++)
                {
                    var watch = Stopwatch.StartNew();
                    int visited = Run(graph, representation, algorithm);
                    watch.Stop();

                    if (expectedVisited == -1) expectedVisited = visited;
                    if (visited != expectedVisited || visited != vertices)
                        throw new InvalidOperationException("Los recorridos no visitaron la misma cantidad de nodos.");

                    double ms = watch.Elapsed.TotalMilliseconds;
                    times.Add(ms);
                    detail.Add(string.Join(",",
                        representation, algorithm, graph.Workers, graph.Posts, vertices, graph.EdgeCount,
                        run, ms.ToString("F6", CultureInfo.InvariantCulture), visited));
                }

                summaries.Add(new Summary(representation, algorithm, n, vertices, graph.EdgeCount,
                    times.Average(), expectedVisited, double.NaN));
            }
        }

        // Factor de crecimiento contra el tamaño anterior para misma representacion y algoritmo.
        for (int i = 0; i < summaries.Count; i++)
        {
            Summary current = summaries[i];
            Summary? previous = summaries.Take(i).LastOrDefault(x =>
                x.Representation == current.Representation && x.Algorithm == current.Algorithm);
            double factor = previous is null || previous.AverageMs <= 0
                ? double.NaN
                : current.AverageMs / previous.AverageMs;
            summaries[i] = current with { Factor = factor };
        }

        var summaryLines = new List<string>
        {
            "Representacion,Algoritmo,Trabajadores,Puestos,Vertices,Aristas,Promedio_ms,Factor_vs_tamano_anterior,NodosVisitados"
        };
        foreach (Summary s in summaries)
        {
            string factor = double.IsNaN(s.Factor) ? "" : s.Factor.ToString("F4", CultureInfo.InvariantCulture);
            summaryLines.Add(string.Join(",", s.Representation, s.Algorithm, s.Workers, s.Workers,
                s.Vertices, s.Edges, s.AverageMs.ToString("F6", CultureInfo.InvariantCulture), factor, s.Visited));
        }

        File.WriteAllLines("grafo_detalle.csv", detail);
        File.WriteAllLines("grafo_resumen.csv", summaryLines);

        Console.WriteLine("Promedios (ms):");
        Console.WriteLine("Representacion Algoritmo Trabajadores Vertices Aristas Promedio Factor");
        foreach (Summary s in summaries)
        {
            string factor = double.IsNaN(s.Factor) ? "-" : s.Factor.ToString("F2", CultureInfo.InvariantCulture);
            Console.WriteLine($"{s.Representation,-14} {s.Algorithm,-9} {s.Workers,11} {s.Vertices,8} {s.Edges,7} {s.AverageMs,9:F6} {factor,7}");
        }
        Console.WriteLine("\nCSV generados: grafo_detalle.csv y grafo_resumen.csv");
        Console.WriteLine("La construccion del grafo queda fuera del cronometro; se mide solo BFS/DFS.");
    }

    private sealed record Summary(string Representation, string Algorithm, int Workers,
        int Vertices, int Edges, double AverageMs, int Visited, double Factor);

    private static GraphData BuildGraph(int n)
    {
        int v = 2 * n; // trabajadores [0,n), puestos [n,2n)
        var adjacency = Enumerable.Range(0, v).Select(_ => new List<int>()).ToArray();
        var matrix = new bool[v, v];
        int edgeCount = 0;

        for (int worker = 0; worker < n; worker++)
        {
            var selectedPosts = new HashSet<int>();
            int attempt = 0;
            while (selectedPosts.Count < Math.Min(EdgesPerWorker, n))
            {
                int stride = n / 2 + 1; // coprimo con estos cuatro tamanos pares
                int post = (worker * 7 + attempt * stride) % n;
                selectedPosts.Add(post);
                attempt++;
            }

            foreach (int postIndex in selectedPosts)
            {
                int postNode = n + postIndex;
                adjacency[worker].Add(postNode);
                adjacency[postNode].Add(worker);
                matrix[worker, postNode] = true;
                matrix[postNode, worker] = true;
                edgeCount++;
            }
        }

        return new GraphData(adjacency, matrix, n, n, edgeCount);
    }

    private static int Run(GraphData graph, string representation, string algorithm)
    {
        return representation == "Lista"
            ? TraverseList(graph.List, algorithm)
            : TraverseMatrix(graph.Matrix, algorithm);
    }

    // Recorre todo el grafo, incluso si tiene varias componentes conexas.
    private static int TraverseList(List<int>[] adjacency, string algorithm)
    {
        int n = adjacency.Length;
        var visited = new bool[n];
        int count = 0;

        for (int start = 0; start < n; start++)
        {
            if (visited[start]) continue;
            if (algorithm == "BFS")
            {
                var queue = new Queue<int>();
                visited[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    count++;
                    foreach (int neighbor in adjacency[current])
                    {
                        if (visited[neighbor]) continue;
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            else
            {
                var stack = new Stack<int>();
                visited[start] = true;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    int current = stack.Pop();
                    count++;
                    foreach (int neighbor in adjacency[current])
                    {
                        if (visited[neighbor]) continue;
                        visited[neighbor] = true;
                        stack.Push(neighbor);
                    }
                }
            }
        }
        return count;
    }

    // En matriz, para encontrar vecinos se examina la fila completa: O(V) por nodo.
    private static int TraverseMatrix(bool[,] matrix, string algorithm)
    {
        int n = matrix.GetLength(0);
        var visited = new bool[n];
        int count = 0;

        for (int start = 0; start < n; start++)
        {
            if (visited[start]) continue;
            if (algorithm == "BFS")
            {
                var queue = new Queue<int>();
                visited[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    count++;
                    for (int neighbor = 0; neighbor < n; neighbor++)
                    {
                        if (!matrix[current, neighbor] || visited[neighbor]) continue;
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            else
            {
                var stack = new Stack<int>();
                visited[start] = true;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    int current = stack.Pop();
                    count++;
                    for (int neighbor = 0; neighbor < n; neighbor++)
                    {
                        if (!matrix[current, neighbor] || visited[neighbor]) continue;
                        visited[neighbor] = true;
                        stack.Push(neighbor);
                    }
                }
            }
        }
        return count;
    }
}

