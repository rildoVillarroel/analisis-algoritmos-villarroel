// CLASE 6 · PRÁCTICA «ELECCIÓN ARGUMENTADA» — Asignación óptima de personal técnico a proyectos eléctricos
//
// Compara tres candidatos para resolver el problema «n trabajadores -> n puestos» minimizando el costo total:
//   A: fuerza bruta (probar todas las permutaciones)      costo  O(n · n!)
//   B: voraz / greedy (ordenar los pares y tomar el mejor) costo  O(n² log n)
//   C: húngaro                                             costo  O(n³)
//
// PROTOCOLO DE MEDICIÓN (el mismo de la clase anterior):
//   · 3 calentamientos por algoritmo
//   · 3 mediciones por algoritmo y tamaño
//   · Promedio = suma de tiempos / 3
//   · Verificación de cada resultado (EsValida, equivalente a EstaOrdenado) FUERA del tiempo medido
//   · Resultados en archivos CSV
//
// Se ejecuta con:  dotnet run -c Release -- eleccion   (o -- datos | duelo | escala | calidad)

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace C6.Eleccion;

// ---------------------------------------------------------------------------------------------
// RESULTADO de un algoritmo. Asignacion[i] = puesto asignado al trabajador i (o -1 si no tiene)
// ---------------------------------------------------------------------------------------------
public sealed record Resultado(string Nombre, double Costo, int Cubiertos, long Operaciones, long Pares, int[] Asignacion);

// ---------------------------------------------------------------------------------------------
// INSTANCIA SINTÉTICA: matriz de costos trabajador x puesto (filas = trabajadores, columnas = puestos)
// ---------------------------------------------------------------------------------------------
public sealed class Instancia
{
    public const double Prohibido = 1_000_000;      // «infinito» práctico para pares NO elegibles

    public int N { get; }
    public double[,] Costo { get; }
    public bool[,] Elegible { get; }
    public int Elegibles { get; }

    // Instancia armada a mano (para los casos pequeños que se resuelven en el pizarrón)
    public Instancia(double[,] costos)
    {
        N = costos.GetLength(0);
        Costo = costos;
        Elegible = new bool[N, N];
        int cuenta = 0;
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
            {
                Elegible[i, j] = costos[i, j] < Prohibido;
                if (Elegible[i, j]) cuenta++;
            }
        Elegibles = cuenta;
    }

    // Instancia generada con el modelo del proyecto:
    //   S = 0.50·S_tec + 0.25·S_exp + 0.15·S_disp + 0.10·S_carga     y     C = 100 - S
    // Restricciones obligatorias: nivel >= nivel mínimo, experiencia >= mínima, horas libres >= horas del puesto.
    public Instancia(int n, int semilla)
    {
        N = n;
        Costo = new double[n, n];
        Elegible = new bool[n, n];
        var rnd = new Random(semilla);

        // Trabajadores: capacidad 40 h/sem y carga actual entre 8 y 16 h  ->  horas libres >= 24
        var carga = new int[n];
        for (int i = 0; i < n; i++) carga[i] = rnd.Next(8, 17);

        // Puestos: nivel mínimo 3-4, experiencia mínima 2-5 años, horas requeridas 16-24
        var nivelMin = new int[n];
        var expMin = new int[n];
        var horas = new int[n];
        int[] horasPosibles = { 16, 18, 20, 24 };
        for (int j = 0; j < n; j++)
        {
            nivelMin[j] = rnd.Next(3, 5);
            expMin[j] = rnd.Next(2, 6);
            horas[j] = horasPosibles[rnd.Next(horasPosibles.Length)];
        }

        // Permutación oculta: garantiza que EXISTE al menos una asignación completa que cumple todo
        var garantizado = new int[n];
        for (int k = 0; k < n; k++) garantizado[k] = k;
        for (int k = n - 1; k > 0; k--)
        {
            int r = rnd.Next(k + 1);
            (garantizado[k], garantizado[r]) = (garantizado[r], garantizado[k]);
        }

        int cuenta = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int nivel = rnd.Next(1, 6);     // nivel del trabajador i en la especialidad del puesto j (1-5)
                int exp = rnd.Next(0, 13);      // años de experiencia en esa especialidad
                if (garantizado[i] == j)
                {
                    nivel = Math.Max(nivel, nivelMin[j]);
                    exp = Math.Max(exp, expMin[j]);
                }
                int libres = 40 - carga[i];

                bool ok = nivel >= nivelMin[j] && exp >= expMin[j] && libres >= horas[j];
                Elegible[i, j] = ok;
                if (ok)
                {
                    double sTec = 100.0 * nivel / 5.0;
                    double sExp = 100.0 * Math.Min(exp / 10.0, 1.0);
                    double sDisp = 100.0 * Math.Min((double)libres / horas[j], 1.0);
                    double sCarga = 100.0 * (1.0 - carga[i] / 40.0);
                    double s = 0.50 * sTec + 0.25 * sExp + 0.15 * sDisp + 0.10 * sCarga;
                    Costo[i, j] = Math.Round(100.0 - s, 2);
                    cuenta++;
                }
                else
                {
                    Costo[i, j] = Prohibido;
                }
            }
        }
        Elegibles = cuenta;
    }
}

// ---------------------------------------------------------------------------------------------
// LOS TRES CANDIDATOS
// ---------------------------------------------------------------------------------------------
public static class Algoritmos
{
    private static int[] SinAsignar(int n)
    {
        var a = new int[n];
        Array.Fill(a, -1);
        return a;
    }

    // ================= A · FUERZA BRUTA =================
    // Se asigna un trabajador libre a cada puesto, puesto por puesto, probando TODAS las permutaciones.
    // Con m trabajadores libres hay m ramas:  L(m) = m · L(m-1) + 1   ->  L(n) = suma de n!/k! ≈ e·n!
    // Cada llamada además recorre n trabajadores, así que el trabajo total es O(n · n!).
    // OJO: NO tiene la forma T(n) = a·T(n/b) + n^d (aquí el problema se achica RESTANDO 1, no dividiendo),
    //      por eso el teorema maestro no aplica; se resuelve desplegando la recurrencia.
    private static long _llamadas;
    private static double _mejor;
    private static int[] _mejorPorPuesto = new int[0];     // _mejorPorPuesto[puesto] = trabajador

    public static Resultado FuerzaBruta(Instancia ins)
    {
        int n = ins.N;
        _llamadas = 0;
        _mejor = double.PositiveInfinity;
        _mejorPorPuesto = SinAsignar(n);
        Probar(ins, 0, new bool[n], new int[n], 0.0);

        bool factible = _mejor < Instancia.Prohibido;
        var asignacion = SinAsignar(n);
        if (factible)
            for (int puesto = 0; puesto < n; puesto++)
                asignacion[_mejorPorPuesto[puesto]] = puesto;

        return new Resultado("FuerzaBruta", factible ? _mejor : double.NaN, factible ? n : 0, _llamadas, 0, asignacion);
    }

    private static void Probar(Instancia ins, int puesto, bool[] usado, int[] actual, double acumulado)
    {
        _llamadas++;
        if (puesto == ins.N)
        {
            if (acumulado < _mejor)
            {
                _mejor = acumulado;
                Array.Copy(actual, _mejorPorPuesto, ins.N);
            }
            return;
        }
        for (int i = 0; i < ins.N; i++)
        {
            if (usado[i]) continue;
            usado[i] = true;
            actual[puesto] = i;
            Probar(ins, puesto + 1, usado, actual, acumulado + ins.Costo[i, puesto]);
            usado[i] = false;
        }
    }

    // ================= B · VORAZ (GREEDY) =================
    // Lista todos los pares elegibles (m <= n²), los ORDENA por costo y toma siempre el más barato
    // cuyo trabajador y cuyo puesto sigan libres.
    // El ordenamiento es lo que pesa: como MergeSort, T(m) = 2·T(m/2) + m  ->  a = 2, b = 2, d = 1
    //   a = b^d  ->  todos los pisos pesan igual  ->  m log m  ->  con m = n²:  O(n² log n)
    // No garantiza el óptimo (ni siquiera garantiza cubrir todos los puestos).
    public static Resultado Voraz(Instancia ins)
    {
        int n = ins.N;
        var pares = new List<(double c, int i, int j)>();
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (ins.Elegible[i, j]) pares.Add((ins.Costo[i, j], i, j));

        long comparaciones = 0;
        pares.Sort((x, y) =>
        {
            comparaciones++;
            int r = x.c.CompareTo(y.c);
            if (r != 0) return r;
            r = x.i.CompareTo(y.i);                 // desempate fijo: el resultado es siempre el mismo
            return r != 0 ? r : x.j.CompareTo(y.j);
        });

        var trabajadorLibre = new bool[n];
        var puestoLibre = new bool[n];
        Array.Fill(trabajadorLibre, true);
        Array.Fill(puestoLibre, true);
        var asignacion = SinAsignar(n);

        double costo = 0;
        int cubiertos = 0;
        foreach (var p in pares)
        {
            if (!trabajadorLibre[p.i] || !puestoLibre[p.j]) continue;
            trabajadorLibre[p.i] = false;
            puestoLibre[p.j] = false;
            asignacion[p.i] = p.j;
            costo += p.c;
            cubiertos++;
            if (cubiertos == n) break;
        }
        return new Resultado("Voraz", costo, cubiertos, comparaciones, pares.Count, asignacion);
    }

    // ================= C · HÚNGARO (versión con potenciales, O(n³)) =================
    // Es iterativo (no se llama a sí mismo): no hay T(n) = a·T(n/b) + n^d que resolver.
    // Para cada fila (n filas) hace como mucho n «pasos de aumento», y cada paso revisa n columnas  ->  n³.
    // Garantiza el costo MÍNIMO para la asignación uno a uno si existe una asignación factible.
    public static Resultado Hungaro(Instancia ins)
    {
        int n = ins.N;
        var a = ins.Costo;
        var u = new double[n + 1];
        var v = new double[n + 1];
        var p = new int[n + 1];          // p[j] = fila asignada a la columna j (1-indexado)
        var way = new int[n + 1];
        long ops = 0;

        for (int i = 1; i <= n; i++)
        {
            p[0] = i;
            int j0 = 0;
            var minv = new double[n + 1];
            Array.Fill(minv, double.PositiveInfinity);
            var usado = new bool[n + 1];
            do
            {
                usado[j0] = true;
                int i0 = p[j0];
                int j1 = 0;
                double delta = double.PositiveInfinity;
                for (int j = 1; j <= n; j++)
                {
                    if (usado[j]) continue;
                    ops++;
                    double cur = a[i0 - 1, j - 1] - u[i0] - v[j];
                    if (cur < minv[j]) { minv[j] = cur; way[j] = j0; }
                    if (minv[j] < delta) { delta = minv[j]; j1 = j; }
                }
                for (int j = 0; j <= n; j++)
                {
                    if (usado[j]) { u[p[j]] += delta; v[j] -= delta; }
                    else minv[j] -= delta;
                }
                j0 = j1;
            } while (p[j0] != 0);

            do
            {
                int j1 = way[j0];
                p[j0] = p[j1];
                j0 = j1;
            } while (j0 != 0);
        }

        var asignacion = SinAsignar(n);
        double costo = 0;
        int cubiertos = 0;
        for (int j = 1; j <= n; j++)
        {
            int fila = p[j] - 1;
            if (ins.Elegible[fila, j - 1])
            {
                asignacion[fila] = j - 1;
                costo += ins.Costo[fila, j - 1];
                cubiertos++;
            }
        }
        return new Resultado("Hungaro", costo, cubiertos, ops, 0, asignacion);
    }

    // ================= VERIFICACIÓN (el «EstaOrdenado» de esta práctica) =================
    // Comprueba que la asignación devuelta es legal: ningún puesto repetido, todos los pares son elegibles
    // y el costo y la cobertura informados coinciden con lo recalculado desde la matriz.
    public static bool EsValida(Instancia ins, Resultado r)
    {
        int n = ins.N;
        if (r.Asignacion.Length != n) return false;

        var puestoUsado = new bool[n];
        double costo = 0;
        int cubiertos = 0;
        for (int i = 0; i < n; i++)
        {
            int j = r.Asignacion[i];
            if (j < 0) continue;
            if (j >= n || puestoUsado[j] || !ins.Elegible[i, j]) return false;
            puestoUsado[j] = true;
            costo += ins.Costo[i, j];
            cubiertos++;
        }
        if (cubiertos != r.Cubiertos) return false;
        return cubiertos == 0 || Math.Abs(costo - r.Costo) < 1e-6;
    }
}

// ---------------------------------------------------------------------------------------------
// EL DUELO: las partes de la práctica
// ---------------------------------------------------------------------------------------------
public static class Duelo
{
    private const int Semilla = 2026;
    private const int Calentamientos = 3;
    private const int Repeticiones = 3;
    private const int LimiteFuerzaBruta = 10;      // más allá de n = 10 la fuerza bruta no entra

    private static readonly int[] TamPequenos = { 4, 5, 6, 7, 8, 9, 10 };
    private static readonly int[] TamGrandes = { 20, 50, 100, 200, 500 };

    private sealed record Candidato(string Nombre, Func<Instancia, Resultado> Ejecutar);

    private static readonly Candidato[] Candidatos =
    {
        new Candidato("FuerzaBruta", Algoritmos.FuerzaBruta),
        new Candidato("Voraz", Algoritmos.Voraz),
        new Candidato("Hungaro", Algoritmos.Hungaro),
    };

    private static void Preparar() => CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

    private static double Fact(int n)
    {
        double f = 1;
        for (int k = 2; k <= n; k++) f *= k;
        return f;
    }

    private static string Num(double x) => x < 1e12 ? x.ToString("N0") : x.ToString("0.00E+0");

    private static string F4(double x) => x.ToString("F4", CultureInfo.InvariantCulture);

    private static string F2(double x) => x.ToString("F2", CultureInfo.InvariantCulture);

    private static bool Participa(Candidato c, int n) => c.Nombre != "FuerzaBruta" || n <= LimiteFuerzaBruta;

    // Cantidad teórica de operaciones que predice el análisis de cada candidato
    private static double Referencia(string algoritmo, int n, long pares)
    {
        if (algoritmo == "FuerzaBruta")
        {
            double suma = 0;
            for (int k = 0; k <= n; k++) suma += Fact(n) / Fact(k);      // llamadas = suma de n!/k! ≈ e·n!
            return suma;
        }
        if (algoritmo == "Voraz") return pares * Math.Log2(Math.Max(pares, 2));   // m log2 m
        return (double)n * n * n;                                                  // n³
    }

    // ---------- PARTE A · MIS DATOS Y LO QUE CUESTA CADA CANDIDATO ----------
    public static void Datos()
    {
        Preparar();
        Console.WriteLine("PARTE A · MIS DATOS Y CUÁNTO CUESTA CADA CANDIDATO (todo exacto: son conteos)");
        Console.WriteLine("Proceso: asignar n trabajadores a n puestos de proyectos eléctricos (datos SINTÉTICOS).");
        Console.WriteLine("Hoy: n = 20 (5 proyectos, 20 puestos).   Peor mes (hipótesis de trabajo): n = 100 (25 proyectos).");
        Console.WriteLine("Operación que más pesa: elegir la combinación de menor costo entre las n! posibles.");
        Console.WriteLine();
        Console.WriteLine($"{"n",5} {"pares n²",10} {"A fuerza bruta e·n!",21} {"B voraz m·log2 m",18} {"C húngaro n³",14}");
        foreach (int n in new[] { 10, 20, 50, 100 })
        {
            double m = (double)n * n;
            Console.WriteLine($"{n,5} {Num(m),10} {Num(Math.E * Fact(n)),21} {Num(m * Math.Log2(m)),18} {Num((double)n * n * n),14}");
        }
        Console.WriteLine();
        Console.WriteLine($"20! = {Fact(20):N0} permutaciones. A 100 millones de permutaciones por segundo (hipótesis generosa):");
        Console.WriteLine($"  tardaría ~{Num(Fact(20) / 1e8 / 31_557_600)} años. El húngaro hace 8.000 pasos de orden n³.");
    }

    // ---------- PARTES B y C · TORNEO CON EL PROTOCOLO (3 calentamientos, 3 mediciones, promedio, verificación, CSV) ----------
    public static void DuelosPequenos() => Protocolo(TamPequenos);      // fuerza bruta contra voraz contra húngaro (n = 4..10)

    public static void Escala() => Protocolo(TamGrandes);               // voraz contra húngaro (n = 20..500)

    private static void Protocolo(int[] tamanos)
    {
        Preparar();
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine(" TORNEO DE ASIGNACIÓN: FUERZA BRUTA · VORAZ · HÚNGARO");
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine($"Tamaños: {string.Join(", ", tamanos)}   (semilla {Semilla}, datos sintéticos)");

        // ==================================
        // FASE 1: CALENTAMIENTO (3 por algoritmo)
        // ==================================
        Console.WriteLine("\nFASE 1: CALENTAMIENTO");
        foreach (var c in Candidatos)
        {
            if (!tamanos.Any(n => Participa(c, n))) continue;
            var datosCalentamiento = new Instancia(c.Nombre == "FuerzaBruta" ? 8 : 50, 99);
            for (int i = 0; i < Calentamientos; i++)
            {
                c.Ejecutar(datosCalentamiento);
                Console.WriteLine($"{c.Nombre} - Calentamiento {i + 1} completado.");
            }
        }

        // ==================================
        // FASE 2: VERIFICACIÓN PREVIA
        // ==================================
        Console.WriteLine("\nFASE 2: VERIFICACIÓN");
        foreach (int n in tamanos)
        {
            var ins = new Instancia(n, Semilla);
            var porNombre = new Dictionary<string, Resultado>();
            foreach (var c in Candidatos.Where(c => Participa(c, n)))
            {
                var r = c.Ejecutar(ins);
                if (!Algoritmos.EsValida(ins, r))
                    throw new Exception($"Error: {c.Nombre} devolvió una asignación inválida (n = {n}).");
                porNombre[c.Nombre] = r;
            }
            if (porNombre["Hungaro"].Cubiertos != n)
                throw new Exception($"Error: el húngaro no cubrió todos los puestos (n = {n}).");
            if (porNombre.ContainsKey("FuerzaBruta") &&
                Math.Abs(porNombre["FuerzaBruta"].Costo - porNombre["Hungaro"].Costo) > 1e-6)
                throw new Exception($"Error: fuerza bruta y húngaro dieron costos distintos (n = {n}).");

            string nota = porNombre.ContainsKey("FuerzaBruta") ? "fuerza bruta = húngaro" : "húngaro cubre todos los puestos";
            Console.WriteLine($"{n} trabajadores: asignaciones válidas ({nota}).");
        }

        // ==================================
        // FASE 3: MEDICIONES
        // ==================================
        Console.WriteLine("\nFASE 3: MEDICIONES");

        var resultados = new List<string> { "Algoritmo,Tamano,Repeticion,Tiempo_ms" };
        var resumen = new List<string> { "Algoritmo,Tamano,Promedio_ms" };
        var conteos = new List<string> { "Algoritmo,Tamano,Pares_elegibles,Operaciones,Referencia_teorica,Cociente,Costo_total,Puestos_cubiertos" };
        var tabla = new List<(string alg, int n, double prom, long ops, double coc, double costo, int cub)>();
        var promedios = new Dictionary<(string, int), double>();

        foreach (int n in tamanos)
        {
            var ins = new Instancia(n, Semilla);
            Console.WriteLine($"\nTAMANO: {n:N0} trabajadores y {n:N0} puestos (pares elegibles: {ins.Elegibles:N0})");

            foreach (var c in Candidatos.Where(c => Participa(c, n)))
            {
                double suma = 0;
                Resultado r = null!;

                for (int rep = 1; rep <= Repeticiones; rep++)
                {
                    // Iniciar cronómetro
                    long t0 = Stopwatch.GetTimestamp();
                    r = c.Ejecutar(ins);
                    double tiempo = Stopwatch.GetElapsedTime(t0).TotalMilliseconds;

                    // Verificar fuera del tiempo medido
                    if (!Algoritmos.EsValida(ins, r))
                        throw new Exception($"Error en {c.Nombre} (n = {n})");

                    suma += tiempo;
                    resultados.Add($"{c.Nombre},{n},{rep},{F4(tiempo)}");
                    Console.WriteLine($"{c.Nombre} - Ejecucion {rep}: {tiempo:F4} ms");
                }

                double promedio = suma / Repeticiones;
                promedios[(c.Nombre, n)] = promedio;
                resumen.Add($"{c.Nombre},{n},{F4(promedio)}");

                double referencia = Referencia(c.Nombre, n, r.Pares);
                double cociente = r.Operaciones / referencia;
                conteos.Add($"{c.Nombre},{n},{ins.Elegibles},{r.Operaciones},{F2(referencia)},{F2(cociente)},{F2(r.Costo)},{r.Cubiertos}");
                tabla.Add((c.Nombre, n, promedio, r.Operaciones, cociente, r.Costo, r.Cubiertos));

                Console.WriteLine($"PROMEDIO {c.Nombre}: {promedio:F4} ms");
            }
        }

        // ==================================
        // FASE 4: MOSTRAR TABLA FINAL
        // ==================================
        Console.WriteLine("\n--------------------------------------------------------------------------------------");
        Console.WriteLine(" TABLA DE PROMEDIOS");
        Console.WriteLine("--------------------------------------------------------------------------------------");
        Console.WriteLine("{0,-12} {1,5} {2,14} {3,16} {4,9} {5,10} {6,9}",
            "ALGORITMO", "n", "PROMEDIO (ms)", "OPERACIONES", "COCIENTE", "COSTO", "CUBRE");
        Console.WriteLine("--------------------------------------------------------------------------------------");
        foreach (var f in tabla)
        {
            Console.WriteLine("{0,-12} {1,5} {2,14:F4} {3,16:N0} {4,9:F2} {5,10:F2} {6,9}",
                f.alg, f.n, f.prom, f.ops, f.coc, f.costo, $"{f.cub}/{f.n}");
        }
        Console.WriteLine();
        Console.WriteLine("COCIENTE = operaciones medidas / operaciones teóricas (e·n! | m·log2 m | n³).");
        Console.WriteLine("Si el cociente se queda en una constante cuando n crece, la forma O(·) predicha es la correcta.");
        Console.WriteLine("CUBRE = puestos cubiertos / puestos requeridos (el voraz puede dejar puestos sin cubrir).");

        if (promedios.TryGetValue(("FuerzaBruta", 10), out double msFb10))
        {
            double tasa = 9_864_101 / (msFb10 / 1000.0);      // llamadas por segundo medidas con n = 10
            double llamadas20 = Math.E * Fact(20);
            Console.WriteLine();
            Console.WriteLine($"Fuerza bruta medida en ESTA máquina: ~{tasa / 1e6:0.0} millones de llamadas por segundo.");
            Console.WriteLine($"Para n = 20 harían falta ~{Num(llamadas20)} llamadas  ->  ~{Num(llamadas20 / tasa / 31_557_600)} años.");
        }

        // ==================================
        // FASE 5: EXPORTAR ARCHIVOS CSV
        // ==================================
        string rutaResultados = Path.Combine(Directory.GetCurrentDirectory(), "resultados.csv");
        string rutaResumen = Path.Combine(Directory.GetCurrentDirectory(), "resumen.csv");
        string rutaConteos = Path.Combine(Directory.GetCurrentDirectory(), "conteos.csv");

        File.WriteAllLines(rutaResultados, resultados);
        File.WriteAllLines(rutaResumen, resumen);
        File.WriteAllLines(rutaConteos, conteos);

        Console.WriteLine("\n======================================");
        Console.WriteLine(" PRUEBA DE ELECCION FINALIZADA");
        Console.WriteLine("======================================");
        Console.WriteLine("\nArchivos generados:");
        Console.WriteLine($"1. {rutaResultados}");
        Console.WriteLine($"2. {rutaResumen}");
        Console.WriteLine($"3. {rutaConteos}");
        Console.WriteLine("\nTodas las asignaciones fueron verificadas.");
    }

    private static void MostrarMini(string titulo, Instancia ins)
    {
        var fb = Algoritmos.FuerzaBruta(ins);
        var vo = Algoritmos.Voraz(ins);
        var hu = Algoritmos.Hungaro(ins);
        Console.WriteLine($"{titulo}: fuerza bruta = {fb.Costo:0.##} | voraz = {vo.Costo:0.##} | húngaro = {hu.Costo:0.##}");
    }

    // ---------- PARTE D · ¿EL MÁS RÁPIDO ES TAMBIÉN EL MEJOR? ----------
    public static void Calidad()
    {
        Preparar();
        Console.WriteLine("PARTE D · CALIDAD DE LA ASIGNACIÓN: rápido no es lo mismo que óptimo");
        Console.WriteLine();

        // El 2×2 que engaña al voraz: toma el 1 y se queda obligado a pagar 100
        MostrarMini("Caso 2×2 [[1,2],[2,100]]      ", new Instancia(new double[,] { { 1, 2 }, { 2, 100 } }));

        // El 3×3 resuelto a mano en el documento del proyecto (X = par no elegible)
        double X = Instancia.Prohibido;
        MostrarMini("Caso 3×3 del proyecto         ", new Instancia(new double[,] { { 17, 31, X }, { 23.5, 31, 16 }, { 36.5, 8, 20 } }));

        Console.WriteLine();
        const int pruebas = 200;
        const int n = 20;
        int completas = 0, incompletas = 0, noOptimas = 0;
        double sumaBrecha = 0, maxBrecha = 0;
        var lineas = new List<string> { "Semilla,Costo_Hungaro,Costo_Voraz,Puestos_cubiertos_voraz,Sobrecosto_pct" };

        for (int semilla = 1; semilla <= pruebas; semilla++)
        {
            var ins = new Instancia(n, semilla);
            var hu = Algoritmos.Hungaro(ins);
            var vo = Algoritmos.Voraz(ins);
            if (!Algoritmos.EsValida(ins, hu) || !Algoritmos.EsValida(ins, vo))
                throw new Exception($"Error: asignación inválida en la semilla {semilla}.");

            if (vo.Cubiertos < n)
            {
                incompletas++;
                lineas.Add($"{semilla},{F2(hu.Costo)},{F2(vo.Costo)},{vo.Cubiertos},NA");
                continue;
            }
            completas++;
            double brecha = (vo.Costo - hu.Costo) / hu.Costo * 100.0;
            if (brecha > 1e-9) noOptimas++;
            sumaBrecha += brecha;
            if (brecha > maxBrecha) maxBrecha = brecha;
            lineas.Add($"{semilla},{F2(hu.Costo)},{F2(vo.Costo)},{vo.Cubiertos},{F2(brecha)}");
        }

        Console.WriteLine($"{pruebas} instancias sintéticas de n = {n} (semillas 1..{pruebas}), húngaro contra voraz:");
        Console.WriteLine($"  voraz NO cubrió todos los puestos : {incompletas} de {pruebas}");
        Console.WriteLine($"  voraz cubrió todo pero NO óptimo  : {noOptimas} de {completas} instancias completas");
        if (completas > 0)
            Console.WriteLine($"  sobrecosto medio del voraz        : {sumaBrecha / completas:0.00} %   (máximo {maxBrecha:0.00} %)");
        Console.WriteLine("El húngaro cubre los 20 puestos y da el mínimo en las 200.");

        string ruta = Path.Combine(Directory.GetCurrentDirectory(), "calidad.csv");
        File.WriteAllLines(ruta, lineas);
        Console.WriteLine($"\nArchivo generado: {ruta}");
    }

    public static void Correr()
    {
        Datos();
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Protocolo(TamPequenos.Concat(TamGrandes).ToArray());
        Console.WriteLine();
        Console.WriteLine(new string('=', 100));
        Calidad();
    }
}
