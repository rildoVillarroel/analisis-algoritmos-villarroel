using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace H3Asignacion;

public static class Program
{
    private static readonly int[] Sizes = { 7, 8, 9, 10 };
    private const int MeasuredRuns = 3;

    public static void Main()
    {
        Console.WriteLine("H3 - Asignacion de trabajadores a puestos");
        Console.WriteLine("Fuerza bruta vs programacion dinamica; puntajes sinteticos.");
        var detail = new List<string> { "Algoritmo,N,Repeticion,Tiempo_ms,PuntajeOptimo" };
        var summary = new List<(string Algorithm, int N, double AverageMs, long Score, double Factor)>();

        foreach (int n in Sizes)
        {
            int[,] scores = BuildScores(n);
            long warmBrute = BruteForce(scores);
            long warmDp = DynamicProgramming(scores);
            if (warmBrute != warmDp) throw new InvalidOperationException($"Optimos distintos en calentamiento n={n}.");

            foreach (string algorithm in new[] { "FuerzaBruta", "Dinamica" })
            {
                var times = new List<double>();
                long expectedScore = -1;
                for (int run = 1; run <= MeasuredRuns; run++)
                {
                    var watch = Stopwatch.StartNew();
                    long score = algorithm == "FuerzaBruta" ? BruteForce(scores) : DynamicProgramming(scores);
                    watch.Stop();
                    if (expectedScore == -1) expectedScore = score;
                    if (score != expectedScore) throw new InvalidOperationException("El optimo cambio entre repeticiones.");
                    double ms = watch.Elapsed.TotalMilliseconds;
                    times.Add(ms);
                    detail.Add($"{algorithm},{n},{run},{ms.ToString("F6", CultureInfo.InvariantCulture)},{score}");
                }
                summary.Add((algorithm, n, times.Average(), expectedScore, double.NaN));
            }
            if (summary[^2].Score != summary[^1].Score)
                throw new InvalidOperationException($"No coincide el optimo para n={n}.");
        }

        for (int i = 0; i < summary.Count; i++)
        {
            var current = summary[i];
            double factor = double.NaN;
            if (i >= 2)
            {
                var previous = summary[i - 2];
                if (previous.AverageMs > 0) factor = current.AverageMs / previous.AverageMs;
            }
            summary[i] = (current.Algorithm, current.N, current.AverageMs, current.Score, factor);
        }

        var summaryLines = new List<string> { "Algoritmo,N,Promedio_ms,Factor_vs_n_anterior,PuntajeOptimo" };
        foreach (var r in summary)
        {
            string factor = double.IsNaN(r.Factor) ? "" : r.Factor.ToString("F4", CultureInfo.InvariantCulture);
            summaryLines.Add($"{r.Algorithm},{r.N},{r.AverageMs.ToString("F6", CultureInfo.InvariantCulture)},{factor},{r.Score}");
        }
        File.WriteAllLines("h3_detalle.csv", detail);
        File.WriteAllLines("h3_resumen.csv", summaryLines);

        Console.WriteLine("Algoritmo       n   Promedio(ms)   Factor    Optimo");
        foreach (var r in summary)
        {
            string factor = double.IsNaN(r.Factor) ? "-" : r.Factor.ToString("F2", CultureInfo.InvariantCulture);
            Console.WriteLine($"{r.Algorithm,-14} {r.N,2} {r.AverageMs,13:F6} {factor,8} {r.Score,9}");
        }
        Console.WriteLine("CSV generados: h3_detalle.csv y h3_resumen.csv");
    }

    // Compatibilidad sintética determinista de 1 a 100 puntos.
    private static int[,] BuildScores(int n)
    {
        var scores = new int[n, n];
        for (int w = 0; w < n; w++)
            for (int p = 0; p < n; p++)
                scores[w, p] = (int)(((w * 37L + p * 19L + w * p * 13L + 17) % 100) + 1);
        return scores;
    }

    // Enumera todas las permutaciones posibles de puestos.
    private static long BruteForce(int[,] scores)
    {
        int n = scores.GetLength(0);
        var used = new bool[n];
        return Search(0, 0);

        long Search(int worker, long accumulated)
        {
            if (worker == n) return accumulated;
            long best = long.MinValue;
            for (int post = 0; post < n; post++)
            {
                if (used[post]) continue;
                used[post] = true;
                long candidate = Search(worker + 1, accumulated + scores[worker, post]);
                if (candidate > best) best = candidate;
                used[post] = false;
            }
            return best;
        }
    }

    // dp[mask] = mejor puntaje asignando los primeros PopCount(mask) trabajadores
    // a los puestos marcados por mask.
    private static long DynamicProgramming(int[,] scores)
    {
        int n = scores.GetLength(0);
        int stateCount = 1 << n;
        long[] dp = new long[stateCount];
        Array.Fill(dp, long.MinValue);
        dp[0] = 0;

        for (int mask = 0; mask < stateCount; mask++)
        {
            if (dp[mask] == long.MinValue) continue;
            int worker = BitOperations.PopCount((uint)mask);
            if (worker >= n) continue;
            for (int post = 0; post < n; post++)
            {
                int bit = 1 << post;
                if ((mask & bit) != 0) continue;
                int nextMask = mask | bit;
                long candidate = dp[mask] + scores[worker, post];
                if (candidate > dp[nextMask]) dp[nextMask] = candidate;
            }
        }
        return dp[stateCount - 1];
    }
}

