// Arboles balanceados aplicados a la asignación de personal.

using System;
using System.Collections.Generic;
using System.Linq;

record Trabajador(string Codigo, string Nombre, string Especialidad, int Experiencia);
record Puesto(string Codigo, string Proyecto, string EspecialidadRequerida, int HorasSemana);

sealed class ComparadorExperiencia : IComparer<Trabajador>
{
    public int Compare(Trabajador? x, Trabajador? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        int c = x.Experiencia.CompareTo(y.Experiencia);
        return c != 0 ? c : StringComparer.Ordinal.Compare(x.Codigo, y.Codigo);
    }
}

class InventarioPersonal
{
    // Hash para búsquedas exactas por código.
    private readonly Dictionary<string, Trabajador> porCodigo = new();
    // SortedSet utiliza un árbol rojinegro balanceado en .NET.
    private readonly SortedSet<Trabajador> porExperiencia = new(new ComparadorExperiencia());
    private readonly Dictionary<string, Puesto> puestos = new();

    public bool InsertarTrabajador(Trabajador t)
    {
        if (porCodigo.ContainsKey(t.Codigo)) return false;
        porCodigo.Add(t.Codigo, t);
        porExperiencia.Add(t);
        return true;
    }

    public Trabajador? BuscarPorCodigo(string codigo)
    {
        porCodigo.TryGetValue(codigo, out var t);
        return t;
    }

    public bool EliminarTrabajador(string codigo)
    {
        if (!porCodigo.TryGetValue(codigo, out var t)) return false;
        porCodigo.Remove(codigo);
        porExperiencia.Remove(t);
        return true;
    }

    public bool InsertarPuesto(Puesto p)
    {
        if (puestos.ContainsKey(p.Codigo)) return false;
        puestos.Add(p.Codigo, p);
        return true;
    }

    public void RecorrerInorden()
    {
        Console.WriteLine("Trabajadores ordenados por experiencia:");
        foreach (var t in porExperiencia)
            Console.WriteLine($"{t.Experiencia,2} años | {t.Codigo} | {t.Nombre} | {t.Especialidad}");
    }

    public void MenorYMayor()
    {
        if (porExperiencia.Count == 0) { Console.WriteLine("No hay trabajadores."); return; }
        Console.WriteLine($"Menor experiencia: {porExperiencia.Min!.Experiencia} años ({porExperiencia.Min.Codigo})");
        Console.WriteLine($"Mayor experiencia: {porExperiencia.Max!.Experiencia} años ({porExperiencia.Max.Codigo})");
    }

    public void ConsultarRango(int desde, int hasta)
    {
        var inferior = new Trabajador("", "", "", desde);
        var superior = new Trabajador("\uffff", "", "", hasta);
        var vista = porExperiencia.GetViewBetween(inferior, superior);
        Console.WriteLine($"Trabajadores entre {desde} y {hasta} años:");
        foreach (var t in vista)
            Console.WriteLine($"{t.Experiencia} años | {t.Codigo} | {t.Nombre}");
        Console.WriteLine($"Cantidad encontrada: {vista.Count}");
    }

    public void Resumen() => Console.WriteLine($"Trabajadores: {porCodigo.Count}; puestos: {puestos.Count}");
}

class Program
{
    static void Main()
    {
        var inv = new InventarioPersonal();
        int[] exp = { 8, 6, 5, 9, 4, 7, 3, 6, 10, 5, 2, 8, 4, 7, 6, 9, 5, 3, 8, 4 };
        string[] esp = { "Media tensión", "Instalaciones hospitalarias", "Automatización", "Mantenimiento eléctrico", "Líneas eléctricas" };
        for (int i = 1; i <= 20; i++)
            inv.InsertarTrabajador(new Trabajador($"T{i:00}", $"Trabajador {i:00}", esp[(i - 1) % 5], exp[i - 1]));

        var puestos = new[] {
            new Puesto("P01", "Línea MT rural", "Media tensión", 40),
            new Puesto("P02", "Hospital de segundo nivel", "Instalaciones hospitalarias", 40),
            new Puesto("P03", "Línea de envasado automatizada", "Automatización", 40),
            new Puesto("P04", "Mantenimiento de celdas MT", "Mantenimiento eléctrico", 32),
            new Puesto("P05", "Centro de eventos/coliseo", "Líneas eléctricas", 40)
        };
        foreach (var p in puestos) inv.InsertarPuesto(p);

        Console.WriteLine("=== ÁRBOLES BALANCEADOS EN EL PROYECTO ==="); inv.Resumen();
        Console.WriteLine("\n1) Búsqueda exacta por hash:");
        var t18 = inv.BuscarPorCodigo("T18");
        Console.WriteLine(t18 is null ? "No encontrado" : $"{t18.Codigo}: {t18.Nombre}, {t18.Experiencia} años");
        Console.WriteLine("\n2) Recorrido inorden:"); inv.RecorrerInorden();
        Console.WriteLine("\n3) Menor y mayor:"); inv.MenorYMayor();
        Console.WriteLine("\n4) Rango de experiencia 5 a 8:"); inv.ConsultarRango(5, 8);
        Console.WriteLine("\n5) Eliminar T18 de ambos índices:");
        Console.WriteLine(inv.EliminarTrabajador("T18") ? "Eliminado" : "No encontrado");
        Console.WriteLine("\n6) Comprobar búsqueda posterior:");
        Console.WriteLine(inv.BuscarPorCodigo("T18") is null ? "T18 ya no existe" : "T18 todavía existe");
        Console.WriteLine("\nResumen final:"); inv.Resumen();
    }
}
