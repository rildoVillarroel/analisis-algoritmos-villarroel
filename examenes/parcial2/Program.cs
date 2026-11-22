using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ClinicaTurnos;

public interface IGestorTurnos
{
    void Registrar(string nombre, int urgencia);
    string? AtenderSiguiente();
    bool EstaEsperando(string nombre);
    int Esperando { get; }
}

public sealed record Paciente(string Nombre, int Urgencia, int Llegada);

// Solucion ingenua: lista simple; seleccionar y eliminar al mas urgente cuesta O(n).
public sealed class GestorIngenuo : IGestorTurnos
{
    private readonly List<Paciente> _lista = new();
    private int _llegadas;

    public void Registrar(string nombre, int urgencia)
    {
        Validar(nombre, urgencia);
        _lista.Add(new Paciente(nombre, urgencia, _llegadas++));
    }

    public string? AtenderSiguiente()
    {
        if (_lista.Count == 0) return null;
        int mejor = 0;
        for (int i = 1; i < _lista.Count; i++)
        {
            Paciente actual = _lista[i];
            Paciente seleccionado = _lista[mejor];
            if (actual.Urgencia < seleccionado.Urgencia ||
                (actual.Urgencia == seleccionado.Urgencia && actual.Llegada < seleccionado.Llegada))
                mejor = i;
        }
        string nombre = _lista[mejor].Nombre;
        _lista.RemoveAt(mejor);
        return nombre;
    }

    public bool EstaEsperando(string nombre)
    {
        foreach (Paciente paciente in _lista)
            if (paciente.Nombre == nombre) return true;
        return false;
    }

    public int Esperando => _lista.Count;

    private static void Validar(string nombre, int urgencia)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre vacio.", nameof(nombre));
        if (urgencia < 1 || urgencia > 5) throw new ArgumentOutOfRangeException(nameof(urgencia));
    }
}

// Solucion mejorada: heap minimo para prioridad y hash para consulta exacta por nombre.
public sealed class GestorMejorado : IGestorTurnos
{
    private readonly PriorityQueue<Paciente, (int Urgencia, int Llegada)> _cola = new();
    private readonly HashSet<string> _nombresEnEspera = new(StringComparer.Ordinal);
    private int _llegadas;

    public void Registrar(string nombre, int urgencia)
    {
        Validar(nombre, urgencia);
        if (_nombresEnEspera.Contains(nombre))
            throw new ArgumentException($"Ya existe un paciente en espera con nombre {nombre}.");
        var paciente = new Paciente(nombre, urgencia, _llegadas++);
        _cola.Enqueue(paciente, (paciente.Urgencia, paciente.Llegada));
        _nombresEnEspera.Add(nombre);
    }

    public string? AtenderSiguiente()
    {
        if (_cola.Count == 0) return null;
        Paciente paciente = _cola.Dequeue();
        _nombresEnEspera.Remove(paciente.Nombre);
        return paciente.Nombre;
    }

    public bool EstaEsperando(string nombre) => _nombresEnEspera.Contains(nombre);
    public int Esperando => _cola.Count;

    private static void Validar(string nombre, int urgencia)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre vacio.", nameof(nombre));
        if (urgencia < 1 || urgencia > 5) throw new ArgumentOutOfRangeException(nameof(urgencia));
    }
}

public static class Programa
{
    private static readonly (string Op, string Arg, int Urgencia, string Esperado)[] Guion =
    {
        ("registrar", "Ana", 3, ""), ("registrar", "Beto", 1, ""),
        ("registrar", "Carla", 3, ""), ("registrar", "Diego", 2, ""),
        ("esperando", "", 0, "4"), ("esta", "Beto", 0, "True"),
        ("esta", "Zoe", 0, "False"), ("atender", "", 0, "Beto"),
        ("esta", "Beto", 0, "False"), ("atender", "", 0, "Diego"),
        ("registrar", "Elena", 1, ""), ("atender", "", 0, "Elena"),
        ("atender", "", 0, "Ana"), ("atender", "", 0, "Carla"),
        ("atender", "", 0, "(nadie)"), ("esperando", "", 0, "0")
    };

    public static void Main()
    {
        Console.WriteLine("=== EJERCICIO PRACTICO: TURNOS DE UNA CLINICA ===");
        Probar("GestorIngenuo", new GestorIngenuo());
        Probar("GestorMejorado", new GestorMejorado());
        Console.Write("N base para medicion [5000]: ");
        string? entrada = Console.ReadLine();
        int nBase = string.IsNullOrWhiteSpace(entrada) ? 5000 : int.Parse(entrada, CultureInfo.InvariantCulture);
        if (nBase < 100) nBase = 5000;
        EjecutarMediciones(nBase);
    }

    private static string Ejecutar(IGestorTurnos g, (string Op, string Arg, int Urgencia, string Esperado) p)
    {
        switch (p.Op)
        {
            case "registrar": g.Registrar(p.Arg, p.Urgencia); return "";
            case "atender": return g.AtenderSiguiente() ?? "(nadie)";
            case "esta": return g.EstaEsperando(p.Arg).ToString();
            default: return g.Esperando.ToString();
        }
    }

    private static void Probar(string titulo, IGestorTurnos g)
    {
        int correctos = 0, total = 0;
        foreach (var p in Guion)
        {
            string obtenido = Ejecutar(g, p);
            if (p.Op == "registrar") continue;
            total++;
            if (obtenido == p.Esperado) correctos++;
            else Console.WriteLine($"ERROR {titulo}: {p.Op} {p.Arg}; esperado={p.Esperado}, obtenido={obtenido}");
        }
        Console.WriteLine($"{titulo}: {correctos} de {total} pasos correctos.");
    }

    public static long EjecutarCarga(IGestorTurnos g, int n)
    {
        for (int i = 0; i < n; i++)
        {
            int urgencia = (int)((i * 7919L) % 5) + 1;
            g.Registrar("P" + i.ToString(CultureInfo.InvariantCulture), urgencia);
        }
        long control = 0;
        for (int i = 0; i < n; i++)
        {
            string? nombre = g.AtenderSiguiente();
            if (nombre is null) throw new InvalidOperationException("No quedan pacientes antes de completar la carga.");
            control += nombre.Length;
        }
        return control;
    }

    private static void EjecutarMediciones(int nBase)
    {
        int[] tamanos = { nBase, checked(nBase * 2), checked(nBase * 4) };
        var detalle = new List<string> { "Estructura,N,Repeticion,Tiempo_ms,Control" };
        var resumen = new List<(string Estructura, int N, double Promedio, double Factor)>();

        foreach (int n in tamanos)
        {
            _ = EjecutarCarga(new GestorIngenuo(), n); // calentamiento
            _ = EjecutarCarga(new GestorMejorado(), n);

            foreach (string estructura in new[] { "Ingenuo", "Mejorado" })
            {
                var tiempos = new List<double>();
                long? controlEsperado = null;
                for (int rep = 1; rep <= 3; rep++)
                {
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                    IGestorTurnos g = estructura == "Ingenuo" ? new GestorIngenuo() : new GestorMejorado();
                    var reloj = Stopwatch.StartNew();
                    long control = EjecutarCarga(g, n);
                    reloj.Stop();
                    controlEsperado ??= control;
                    if (control != controlEsperado.Value) throw new InvalidOperationException("Control distinto entre repeticiones.");
                    double ms = reloj.Elapsed.TotalMilliseconds;
                    tiempos.Add(ms);
                    detalle.Add($"{estructura},{n},{rep},{ms.ToString("F4", CultureInfo.InvariantCulture)},{control}");
                }
                resumen.Add((estructura, n, tiempos.Average(), double.NaN));
            }
        }

        for (int i = 0; i < resumen.Count; i++)
        {
            var actual = resumen[i];
            double factor = double.NaN;
            if (i >= 2 && resumen[i - 2].Promedio > 0)
                factor = actual.Promedio / resumen[i - 2].Promedio;
            resumen[i] = (actual.Estructura, actual.N, actual.Promedio, factor);
        }

        var salida = new List<string> { "Estructura,N,Promedio_ms,Factor_vs_tamano_anterior" };
        foreach (var r in resumen)
        {
            string f = double.IsNaN(r.Factor) ? "" : r.Factor.ToString("F3", CultureInfo.InvariantCulture);
            salida.Add($"{r.Estructura},{r.N},{r.Promedio.ToString("F4", CultureInfo.InvariantCulture)},{f}");
        }
        File.WriteAllLines("mediciones_detalle.csv", detalle);
        File.WriteAllLines("mediciones_resumen.csv", salida);

        Console.WriteLine("\nEstructura  N          Promedio(ms)   Factor");
        foreach (var r in resumen)
            Console.WriteLine($"{r.Estructura,-11} {r.N,-10} {r.Promedio,12:F4}   {(double.IsNaN(r.Factor) ? "-" : r.Factor.ToString("F2", CultureInfo.InvariantCulture))}");
        Console.WriteLine("CSV generados: mediciones_detalle.csv y mediciones_resumen.csv");
        Console.WriteLine("Verifica que el promedio ingenuo para 4n este entre 500 y 20.000 ms.");
        Console.WriteLine("Si es menor, aumenta N base; si supera 20.000 ms, reduce N base.");
    }
}
