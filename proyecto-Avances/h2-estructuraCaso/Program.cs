// MEDICIÓN · ASIGNACIÓN OPTIMA DE PERSONAL A PROYECTOS ELÉCTRICOS: estructura INGENUA contra estructura ADECUADA
//
// DATOS 100 % SINTÉTICOS (semilla fija). No son trabajadores ni proyectos reales.
//
// EL PROBLEMA (el mismo del proyecto, versión voraz con equilibrio de carga):
//   Hay un inventario de n trabajadores y n/4 puestos por cubrir. Para cada puesto, en orden:
//     1) buscar los trabajadores que lo cumplen: perfil igual, nivel >= mínimo, experiencia >= mínima
//        y horas libres >= horas del puesto (libres = 40 - carga);
//     2) elegir entre ellos al de MENOR carga (desempate: el de menor Id);
//     3) retirarlo del inventario (un trabajador ocupa un solo puesto).
//   Si nadie cumple, el puesto queda sin cubrir.
//   Operaciones del inventario que se usan: rango/umbral, el menor, el siguiente, borrar (y armar la estructura).
//
// CONTENDIENTES (todos dan EXACTAMENTE la misma asignación; el tiempo incluye ARMAR la estructura):
//   Lista            INGENUA   List<Trabajador> sin orden; cada puesto recorre todo el inventario.
//   HashPorPerfil    control   Dictionary perfil -> List; cada puesto recorre solo la lista de su perfil.
//   ArbolSinFiltro   control   Dictionary perfil -> SortedSet ordenado por carga (sin filtro de habilitados).
//   ArbolPorPerfil   ADECUADA  Igual, pero solo se indexa al personal HABILITADO (nivel >= 3 y exp >= 2):
//                              ningún puesto del proyecto exige menos, así que los demás nunca son elegibles.
//
// PROTOCOLO (el de la materia): 3 calentamientos por contendiente, 3 mediciones por contendiente y tamaño,
// promedio = suma / 3, verificación fuera del tiempo medido, resultados en CSV.
//
// ARCHIVOS: resultados.csv (cada corrida) · resumen.csv (promedios, factor, dispersión) ·
//           cobertura.csv (puestos cubiertos) · tabla_medicion.md (la tabla lista para pegar en medicion.md)
//
// Ejecutar:  dotnet run -c Release      (tarda cerca de un minuto)

using System.Diagnostics;
using System.Globalization;

class Program
{
    static readonly int[] Reales = { 20, 100 };                         // hoy y peor mes (hipótesis)
    static readonly int[] Doblados = { 10_000, 20_000, 40_000, 80_000 }; // tamaños de estrés que se duplican
    const int Calentamientos = 3;
    const int Repeticiones = 3;

    // Cada algoritmo deja aquí su asignación (puesto k -> Id del trabajador, o -1) para poder verificarla
    static int[] _ultima = Array.Empty<int>();

    static readonly Contendiente[] Contendientes =
    {
        new("Lista",          "ingenua",  d => () => AsignarLista(d)),
        new("HashPorPerfil",  "control",  d => () => AsignarHash(d)),
        new("ArbolSinFiltro", "control",  d => () => AsignarArbol(d, false)),
        new("ArbolPorPerfil", "adecuada", d => () => AsignarArbol(d, true)),
    };

    static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        int[] tamanos = Reales.Concat(Doblados).ToArray();

        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine(" ASIGNACION DE PERSONAL: ESTRUCTURA INGENUA / ADECUADA");
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine($"Tamanos: {string.Join(", ", tamanos.Select(t => t.ToString("N0")))}  (puestos = n / 4)");

        // ==================================
        // FASE 1: CALENTAMIENTO (3 por contendiente)
        // ==================================
        Console.WriteLine("\nFASE 1: CALENTAMIENTO");
        var datosCalentamiento = new Datos(2000);
        foreach (var c in Contendientes)
        {
            for (int i = 0; i < Calentamientos; i++)
                c.Preparar(datosCalentamiento)();
            Console.WriteLine($"{c.Estructura}: {Calentamientos} calentamientos completados.");
        }

        // ==================================
        // FASE 2: VERIFICACION PREVIA
        // ==================================
        Console.WriteLine("\nFASE 2: VERIFICACION");
        var esperados = new Dictionary<int, long>();
        var cobertura = new List<string> { "Tamano,Puestos,Cubiertos,Sin_cubrir" };

        foreach (int n in tamanos)
        {
            var datos = new Datos(n);

            // El recorrido inorden de cada árbol debe salir ordenado
            foreach (var arbol in ConstruirArbol(datos, true).Values)
                if (!EstaOrdenado(arbol))
                    throw new Exception($"Error: un arbol no sale ordenado (n = {n}).");

            long? referencia = null;
            int cubiertos = 0;
            foreach (var c in Contendientes)
            {
                long firma = c.Preparar(datos)();

                if (!AsignacionValida(datos, _ultima))
                    throw new Exception($"Error: {c.Estructura} dio una asignacion invalida (n = {n}).");

                if (referencia == null)
                {
                    referencia = firma;
                    cubiertos = _ultima.Count(id => id >= 0);
                }
                else if (firma != referencia)
                {
                    throw new Exception($"Error: {c.Estructura} dio una asignacion distinta a la de la lista (n = {n}).");
                }
            }
            esperados[n] = referencia!.Value;
            cobertura.Add($"{n},{datos.Puestos.Length},{cubiertos},{datos.Puestos.Length - cubiertos}");
            Console.WriteLine($"{n,8:N0} trabajadores, {datos.Puestos.Length,6:N0} puestos: las 4 estructuras dan la MISMA asignacion valida ({cubiertos:N0} puestos cubiertos).");
        }

        // ==================================
        // FASE 3: MEDICIONES
        // ==================================
        Console.WriteLine("\nFASE 3: MEDICIONES");

        var resultados = new List<string> { "Estructura,Tamano,Repeticion,Tiempo_ms" };
        var resumen = new List<string> { "Estructura,Rol,Tamano,Puestos,Promedio_ms,Factor_vs_fila_anterior,Min_ms,Max_ms,Dispersion_pct" };
        var promedios = new Dictionary<(string, int), double>();
        var factores = new Dictionary<(string, int), string>();
        var dispersiones = new Dictionary<(string, int), double>();
        var tabla = new List<(string est, string rol, int n, double prom, string factor, double disp)>();

        foreach (int n in tamanos)
        {
            var datos = new Datos(n);
            Console.WriteLine($"\nTAMANO: {n:N0} trabajadores, {datos.Puestos.Length:N0} puestos");

            foreach (var c in Contendientes)
            {
                double suma = 0, minimo = double.MaxValue, maximo = 0;

                for (int rep = 1; rep <= Repeticiones; rep++)
                {
                    var accion = c.Preparar(datos);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Iniciar cronometro (incluye ARMAR la estructura + resolver todos los puestos)
                    Stopwatch reloj = Stopwatch.StartNew();
                    long firma = accion();
                    reloj.Stop();

                    // Verificar fuera del tiempo medido
                    if (firma != esperados[n])
                        throw new Exception($"Error en {c.Estructura} (n = {n}).");

                    double tiempo = reloj.Elapsed.TotalMilliseconds;
                    suma += tiempo;
                    minimo = Math.Min(minimo, tiempo);
                    maximo = Math.Max(maximo, tiempo);

                    resultados.Add($"{c.Estructura},{n},{rep},{tiempo.ToString("F4", CultureInfo.InvariantCulture)}");
                    Console.WriteLine($"{c.Estructura,-15} - Ejecucion {rep}: {tiempo:F4} ms");
                }

                double promedio = suma / Repeticiones;
                double dispersion = promedio > 0 ? (maximo - minimo) / promedio * 100.0 : 0;
                promedios[(c.Estructura, n)] = promedio;
                dispersiones[(c.Estructura, n)] = dispersion;

                // Factor contra la fila anterior: solo cuando el tamano se duplica
                string factor = "NA";
                if (promedios.TryGetValue((c.Estructura, n / 2), out double anterior) && anterior > 0)
                    factor = (promedio / anterior).ToString("F2", CultureInfo.InvariantCulture);
                factores[(c.Estructura, n)] = factor;

                resumen.Add(
                    $"{c.Estructura},{c.Rol},{n},{datos.Puestos.Length}," +
                    $"{promedio.ToString("F4", CultureInfo.InvariantCulture)},{factor}," +
                    $"{minimo.ToString("F4", CultureInfo.InvariantCulture)},{maximo.ToString("F4", CultureInfo.InvariantCulture)}," +
                    $"{dispersion.ToString("F1", CultureInfo.InvariantCulture)}");
                tabla.Add((c.Estructura, c.Rol, n, promedio, factor, dispersion));

                Console.WriteLine($"PROMEDIO {c.Estructura}: {promedio:F4} ms   (dispersion entre corridas: {dispersion:F1} %)");
            }
        }

        // ==================================
        // FASE 4: MOSTRAR TABLA FINAL
        // ==================================
        Console.WriteLine("\n-----------------------------------------------------------------------------");
        Console.WriteLine(" TABLA DE PROMEDIOS (factor = cuantas veces mas tarda que con la mitad de datos)");
        Console.WriteLine("-----------------------------------------------------------------------------");
        Console.WriteLine("{0,-15} {1,-9} {2,9} {3,14} {4,8} {5,12}", "ESTRUCTURA", "ROL", "n", "PROMEDIO (ms)", "FACTOR", "DISPERSION");
        Console.WriteLine("-----------------------------------------------------------------------------");
        foreach (var f in tabla)
            Console.WriteLine("{0,-15} {1,-9} {2,9:N0} {3,14:F4} {4,8} {5,11:F1}%", f.est, f.rol, f.n, f.prom, f.factor, f.disp);

        Console.WriteLine("\nCUANTAS VECES MAS LENTA ES LA INGENUA (Lista) QUE LA ADECUADA (ArbolPorPerfil):");
        foreach (int n in tamanos)
        {
            double ti = promedios[("Lista", n)];
            double ta = promedios[("ArbolPorPerfil", n)];
            string veces = ta > 0 ? $"{ti / ta:N1} veces" : "(tiempo demasiado pequeno)";
            Console.WriteLine($"  n = {n,8:N0}: {veces}");
        }

        var ruidosas = tabla.Where(f => f.prom >= 1.0).OrderByDescending(f => f.disp).FirstOrDefault();
        if (ruidosas.est != null)
            Console.WriteLine($"\nMayor dispersion entre las 3 corridas (en tiempos de 1 ms o mas): {ruidosas.disp:F1} % ({ruidosas.est}, n = {ruidosas.n:N0}).");
        Console.WriteLine("Si con n = 20 o 100 la lista gana o empata, la estructura sofisticada no compensa en tu caso real.");

        // ==================================
        // FASE 5: EXPORTAR ARCHIVOS
        // ==================================
        var md = ConstruirTablaMarkdown(promedios, factores, dispersiones);

        string rutaResultados = Path.Combine(Directory.GetCurrentDirectory(), "resultados.csv");
        string rutaResumen = Path.Combine(Directory.GetCurrentDirectory(), "resumen.csv");
        string rutaCobertura = Path.Combine(Directory.GetCurrentDirectory(), "cobertura.csv");
        string rutaTabla = Path.Combine(Directory.GetCurrentDirectory(), "tabla_medicion.md");

        File.WriteAllLines(rutaResultados, resultados);
        File.WriteAllLines(rutaResumen, resumen);
        File.WriteAllLines(rutaCobertura, cobertura);
        File.WriteAllLines(rutaTabla, md);

        Console.WriteLine("\n======================================");
        Console.WriteLine(" PRUEBA FINALIZADA");
        Console.WriteLine("======================================");
        Console.WriteLine("\nArchivos generados:");
        Console.WriteLine($"1. {rutaResultados}");
        Console.WriteLine($"2. {rutaResumen}");
        Console.WriteLine($"3. {rutaCobertura}");
        Console.WriteLine($"4. {rutaTabla}   (pegala en medicion.md)");
        Console.WriteLine("\nTodos los resultados fueron verificados.");
    }

    // =====================================================================================
    // ELEGIBILIDAD: las restricciones obligatorias del puesto
    // =====================================================================================

    static bool Elegible(Trabajador w, Puesto p) =>
        w.Perfil == p.Perfil && w.Nivel >= p.NivelMin && w.Exp >= p.ExpMin && w.Libres >= p.Horas;

    // =====================================================================================
    // INGENUA · Lista sin orden. Por puesto: O(n) para buscar al de menor carga + O(n) para retirarlo.
    // Total: O(p · n), con p proporcional a n  ->  cuadratico.
    // =====================================================================================

    static long AsignarLista(Datos d)
    {
        var lista = new List<Trabajador>(d.Inventario);          // copia de trabajo (cuenta como armar)
        var salida = new int[d.Puestos.Length];
        long firma = 0;

        for (int k = 0; k < d.Puestos.Length; k++)
        {
            var puesto = d.Puestos[k];
            int mejor = -1;
            for (int i = 0; i < lista.Count; i++)                // recorre TODO el inventario
            {
                var candidato = lista[i];
                if (!Elegible(candidato, puesto)) continue;
                if (mejor < 0 || candidato.Carga < lista[mejor].Carga) mejor = i;   // estricto: gana el de menor Id
            }

            if (mejor < 0)
            {
                salida[k] = -1;
                firma = firma * 31;
            }
            else
            {
                var elegido = lista[mejor];
                salida[k] = elegido.Id;
                firma = firma * 31 + elegido.Id + 1;
                lista.RemoveAt(mejor);                            // O(n): desplaza el resto
            }
        }

        _ultima = salida;
        return firma;
    }

    // =====================================================================================
    // CONTROL · Hash por perfil. Por puesto: O(n / perfiles) (solo mira a su perfil). Sigue siendo cuadratico,
    // pero con una constante unas 14 veces menor.
    // =====================================================================================

    static long AsignarHash(Datos d)
    {
        var porPerfil = new Dictionary<int, List<Trabajador>>();
        foreach (var w in d.Inventario)
        {
            if (!porPerfil.TryGetValue(w.Perfil, out var grupo))
            {
                grupo = new List<Trabajador>();
                porPerfil[w.Perfil] = grupo;
            }
            grupo.Add(w);
        }

        var salida = new int[d.Puestos.Length];
        long firma = 0;

        for (int k = 0; k < d.Puestos.Length; k++)
        {
            var puesto = d.Puestos[k];
            int mejor = -1;
            porPerfil.TryGetValue(puesto.Perfil, out var lista);

            if (lista != null)
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    var candidato = lista[i];
                    if (!Elegible(candidato, puesto)) continue;
                    if (mejor < 0 || candidato.Carga < lista[mejor].Carga) mejor = i;
                }
            }

            if (mejor < 0 || lista == null)
            {
                salida[k] = -1;
                firma = firma * 31;
            }
            else
            {
                var elegido = lista[mejor];
                salida[k] = elegido.Id;
                firma = firma * 31 + elegido.Id + 1;
                lista.RemoveAt(mejor);
            }
        }

        _ultima = salida;
        return firma;
    }

    // =====================================================================================
    // ADECUADA · Árbol rojinegro (SortedSet) por perfil, ordenado por carga.
    //   Armar:   n inserciones de O(log n)  ->  O(n log n)
    //   Por puesto: «el menor» y «el siguiente» recorriendo en orden de carga, con tope de carga (rango por umbral
    //   de horas libres), y «borrar» al elegido en O(log n).
    //   Con el filtro de habilitados cada puesto mira muy pocos nodos; sin el filtro, los no habilitados se
    //   acumulan al inicio de cada árbol y hay que saltarlos una y otra vez.
    // =====================================================================================

    static Dictionary<int, SortedSet<(int carga, int id, int nivel, int exp)>> ConstruirArbol(Datos d, bool soloHabilitados)
    {
        var porPerfil = new Dictionary<int, SortedSet<(int carga, int id, int nivel, int exp)>>();
        foreach (var w in d.Inventario)
        {
            if (soloHabilitados && (w.Nivel < Datos.NivelMinimoGlobal || w.Exp < Datos.ExpMinimaGlobal)) continue;

            if (!porPerfil.TryGetValue(w.Perfil, out var arbol))
            {
                arbol = new SortedSet<(int carga, int id, int nivel, int exp)>();
                porPerfil[w.Perfil] = arbol;
            }
            arbol.Add((w.Carga, w.Id, w.Nivel, w.Exp));            // O(log n)
        }
        return porPerfil;
    }

    static long AsignarArbol(Datos d, bool soloHabilitados)
    {
        var porPerfil = ConstruirArbol(d, soloHabilitados);
        var salida = new int[d.Puestos.Length];
        long firma = 0;

        for (int k = 0; k < d.Puestos.Length; k++)
        {
            var puesto = d.Puestos[k];
            bool hay = false;
            (int carga, int id, int nivel, int exp) elegido = default;

            if (porPerfil.TryGetValue(puesto.Perfil, out var arbol))
            {
                int tope = 40 - puesto.Horas;                       // horas libres suficientes <=> carga <= tope
                foreach (var t in arbol)                            // en orden de carga: el menor, el siguiente, ...
                {
                    if (t.carga > tope) break;                      // ya nadie más cumple las horas
                    if (t.nivel >= puesto.NivelMin && t.exp >= puesto.ExpMin) { elegido = t; hay = true; break; }
                }

                if (hay) arbol.Remove(elegido);                     // borrar: O(log n) (fuera del foreach)
            }

            if (!hay)
            {
                salida[k] = -1;
                firma = firma * 31;
            }
            else
            {
                salida[k] = elegido.id;
                firma = firma * 31 + elegido.id + 1;
            }
        }

        _ultima = salida;
        return firma;
    }

    // =====================================================================================
    // VERIFICACIONES
    // =====================================================================================

    // Cada trabajador se usa a lo sumo una vez y cumple las restricciones del puesto al que se asignó
    static bool AsignacionValida(Datos d, int[] salida)
    {
        if (salida.Length != d.Puestos.Length) return false;
        var usado = new bool[d.N];
        for (int k = 0; k < salida.Length; k++)
        {
            int id = salida[k];
            if (id < 0) continue;
            if (id >= d.N || usado[id]) return false;
            usado[id] = true;
            if (!Elegible(d.Inventario[id], d.Puestos[k])) return false;     // Id = posicion en el inventario
        }
        return true;
    }

    // El «EstaOrdenado» de esta práctica: el recorrido inorden de un árbol sale de menor a mayor
    static bool EstaOrdenado<T>(IEnumerable<T> secuencia) where T : IComparable<T>
    {
        bool primero = true;
        T anterior = default!;
        foreach (T actual in secuencia)
        {
            if (!primero && anterior.CompareTo(actual) > 0) return false;
            anterior = actual;
            primero = false;
        }
        return true;
    }

    // =====================================================================================
    // TABLA LISTA PARA PEGAR EN medicion.md
    // =====================================================================================

    static List<string> ConstruirTablaMarkdown(
        Dictionary<(string, int), double> promedios,
        Dictionary<(string, int), string> factores,
        Dictionary<(string, int), double> dispersiones)
    {
        string Ms(double x) => x.ToString("F4", CultureInfo.InvariantCulture);
        string Fx(string est, int n) => factores[(est, n)] == "NA" ? "—" : "×" + factores[(est, n)];

        var md = new List<string>();

        md.Add("### Tamaños que se duplican (promedio de 3 corridas en ms; × = veces que tarda más que la fila anterior)");
        md.Add("");
        md.Add("| n | Puestos | Lista (ms) | × | Árbol por perfil (ms) | × | Lista / Árbol | Dispersión Lista | Dispersión Árbol |");
        md.Add("|---|---|---|---|---|---|---|---|---|");
        foreach (int n in Doblados)
        {
            double tl = promedios[("Lista", n)];
            double ta = promedios[("ArbolPorPerfil", n)];
            string veces = ta > 0 ? (tl / ta).ToString("N1", CultureInfo.InvariantCulture) + " veces" : "—";
            md.Add($"| {n:N0} | {n / 4:N0} | {Ms(tl)} | {Fx("Lista", n)} | {Ms(ta)} | {Fx("ArbolPorPerfil", n)} | {veces} | " +
                   $"{dispersiones[("Lista", n)]:F1} % | {dispersiones[("ArbolPorPerfil", n)]:F1} % |");
        }

        md.Add("");
        md.Add("### Controles (mismos tamaños)");
        md.Add("");
        md.Add("| n | Hash por perfil (ms) | × | Árbol sin filtro (ms) | × |");
        md.Add("|---|---|---|---|---|");
        foreach (int n in Doblados)
            md.Add($"| {n:N0} | {Ms(promedios[("HashPorPerfil", n)])} | {Fx("HashPorPerfil", n)} | {Ms(promedios[("ArbolSinFiltro", n)])} | {Fx("ArbolSinFiltro", n)} |");

        md.Add("");
        md.Add("### Tamaños reales de mi caso (no se duplican, no llevan factor)");
        md.Add("");
        md.Add("| n | Puestos | Lista (ms) | Hash por perfil (ms) | Árbol sin filtro (ms) | Árbol por perfil (ms) |");
        md.Add("|---|---|---|---|---|---|");
        foreach (int n in Reales)
            md.Add($"| {n} | {n / 4} | {Ms(promedios[("Lista", n)])} | {Ms(promedios[("HashPorPerfil", n)])} | {Ms(promedios[("ArbolSinFiltro", n)])} | {Ms(promedios[("ArbolPorPerfil", n)])} |");

        return md;
    }
}

// =========================================================================================
// TIPOS
// =========================================================================================

sealed class Trabajador
{
    public int Id;
    public int Perfil;                      // 0..13 (responsable, especialista MT, liniero, instalador BT, ...)
    public int Nivel;                       // 1-5
    public int Exp;                         // años de experiencia
    public int Carga;                       // h/sem ya comprometidas
    public int Libres => 40 - Carga;        // capacidad de 40 h/sem
}

readonly record struct Puesto(int Perfil, int NivelMin, int ExpMin, int Horas);

// Un contendiente = una estructura. Preparar(datos) devuelve la acción que se cronometra (armar + resolver).
sealed record Contendiente(string Estructura, string Rol, Func<Datos, Func<long>> Preparar);

sealed class Datos
{
    public const int Perfiles = 14;
    public const int NivelMinimoGlobal = 3;     // ningún puesto exige menos de nivel 3 ...
    public const int ExpMinimaGlobal = 2;       // ... ni menos de 2 años de experiencia

    public int N { get; }
    public List<Trabajador> Inventario { get; }
    public Puesto[] Puestos { get; }

    public Datos(int n, int semilla = 2026)
    {
        N = n;
        var azar = new Random(semilla);

        // Los trabajadores llegan con Id ya ordenado (0, 1, 2, ...) y su Id es su posición en la lista
        Inventario = new List<Trabajador>(n);
        for (int i = 0; i < n; i++)
        {
            Inventario.Add(new Trabajador
            {
                Id = i,
                Perfil = azar.Next(Perfiles),
                Nivel = azar.Next(1, 6),
                Exp = azar.Next(0, 16),
                Carga = azar.Next(4, 29),
            });
        }

        // n / 4 puestos; cada uno exige nivel 3-4, experiencia 2-5 años y 16-24 horas
        int[] horas = { 16, 18, 20, 24 };
        int cantidad = Math.Max(1, n / 4);
        Puestos = new Puesto[cantidad];
        for (int k = 0; k < cantidad; k++)
            Puestos[k] = new Puesto(azar.Next(Perfiles), azar.Next(NivelMinimoGlobal, 5), azar.Next(ExpMinimaGlobal, 6), horas[azar.Next(horas.Length)]);
    }
}
