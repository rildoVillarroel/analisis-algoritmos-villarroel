using System;
using System.Collections.Generic;
using System.Linq;

record Trabajador(string Codigo, string Nombre, string Especialidad, int Experiencia);
record Puesto(string Codigo, string Proyecto, string Especialidad, int HorasSemana);
record Asignacion(string Trabajador, string Puesto, string Proyecto);

class Inventario
{
    private readonly Dictionary<string, Trabajador> trabajadores = new();
    private readonly Dictionary<string, Puesto> puestos = new();
    private readonly HashSet<string> asignados = new();
    private readonly SortedDictionary<string, Trabajador> ordenados = new();
    private readonly List<Asignacion> asignaciones = new();

    public bool InsertarTrabajador(Trabajador t)
    {
        if (trabajadores.ContainsKey(t.Codigo)) return false;
        trabajadores.Add(t.Codigo, t);
        ordenados.Add(t.Codigo, t);
        return true;
    }

    public bool BuscarTrabajador(string codigo, out Trabajador? t) =>
        trabajadores.TryGetValue(codigo, out t);

    public bool BorrarTrabajador(string codigo)
    {
        if (asignados.Contains(codigo)) return false;
        ordenados.Remove(codigo);
        return trabajadores.Remove(codigo);
    }

    public bool InsertarPuesto(Puesto p)
    {
        if (puestos.ContainsKey(p.Codigo)) return false;
        puestos.Add(p.Codigo, p);
        return true;
    }

    public bool RegistrarAsignacion(Asignacion a)
    {
        if (!trabajadores.ContainsKey(a.Trabajador) ||
            !puestos.ContainsKey(a.Puesto) ||
            asignados.Contains(a.Trabajador) ||
            asignaciones.Any(x => x.Puesto == a.Puesto))
            return false;

        asignaciones.Add(a);
        asignados.Add(a.Trabajador);
        return true;
    }

    public bool QuitarAsignacion(string codigoTrabajador)
    {
        int i = asignaciones.FindIndex(x => x.Trabajador == codigoTrabajador);
        if (i < 0) return false;
        asignaciones.RemoveAt(i);
        asignados.Remove(codigoTrabajador);
        return true;
    }

    public void RecorrerOrdenados()
    {
        foreach (var t in ordenados.Values)
            Console.WriteLine($"{t.Codigo} | {t.Nombre} | {t.Especialidad}");
    }

    public void MenorYMayor()
    {
        if (ordenados.Count == 0) { Console.WriteLine("Sin trabajadores"); return; }
        Console.WriteLine($"Menor código: {ordenados.First().Key}");
        Console.WriteLine($"Mayor código: {ordenados.Last().Key}");
    }

    public void Rango(string desde, string hasta)
    {
        foreach (var par in ordenados)
            if (string.CompareOrdinal(par.Key, desde) >= 0 &&
                string.CompareOrdinal(par.Key, hasta) <= 0)
                Console.WriteLine($"{par.Key} | {par.Value.Nombre}");
    }

    public void Resumen() =>
        Console.WriteLine($"Trabajadores: {trabajadores.Count}; Puestos: {puestos.Count}; Asignaciones: {asignaciones.Count}");
}

class Program
{
    static void Main()
    {
        var inv = new Inventario();
        string[] especialidades = { "Media tensión", "Instalaciones hospitalarias", "Automatización", "Mantenimiento eléctrico", "Líneas eléctricas" };

        for (int i = 1; i <= 20; i++)
        {
            string codigo = $"T{i:00}";
            inv.InsertarTrabajador(new Trabajador(codigo, $"Trabajador {i:00}", especialidades[(i - 1) % 5], 2 + i % 9));
        }

        var puestos = new[]
        {
            new Puesto("P01", "Línea MT rural", "Media tensión", 40),
            new Puesto("P02", "Hospital de segundo nivel", "Instalaciones hospitalarias", 40),
            new Puesto("P03", "Línea de envasado automatizada", "Automatización", 40),
            new Puesto("P04", "Mantenimiento de celdas MT", "Mantenimiento eléctrico", 32),
            new Puesto("P05", "Centro de eventos/coliseo", "Líneas eléctricas", 40)
        };
        foreach (var p in puestos) inv.InsertarPuesto(p);

        Console.WriteLine("INVENTARIO DEL PROYECTO");
        inv.Resumen();

        Console.WriteLine("\nBuscar T18 (Dictionary):");
        if (inv.BuscarTrabajador("T18", out var t))
            Console.WriteLine($"{t!.Codigo}: {t.Nombre}, {t.Especialidad}");

        Console.WriteLine("\nRegistrar T18 en P03:");
        Console.WriteLine(inv.RegistrarAsignacion(new Asignacion("T18", "P03", "Línea de envasado automatizada")) ? "Registrada" : "Rechazada");

        Console.WriteLine("\nIntentar borrar T18 asignado:");
        Console.WriteLine(inv.BorrarTrabajador("T18") ? "Borrado" : "No se puede borrar: tiene asignación activa");

        Console.WriteLine("\nRecorrido ordenado:");
        inv.RecorrerOrdenados();

        Console.WriteLine("\nMenor y mayor:");
        inv.MenorYMayor();

        Console.WriteLine("\nRango T05 a T10:");
        inv.Rango("T05", "T10");

        Console.WriteLine("\nLiberar T18 y borrarlo:");
        inv.QuitarAsignacion("T18");
        Console.WriteLine(inv.BorrarTrabajador("T18") ? "Trabajador eliminado" : "No se pudo eliminar");

        Console.WriteLine("\nResumen final:");
        inv.Resumen();
    }
}
