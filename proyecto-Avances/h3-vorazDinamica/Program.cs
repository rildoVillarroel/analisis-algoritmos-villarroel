
// H3 · ¿VORAZ O DINÁMICA? — Asignación óptima de personal técnico a proyectos eléctricos

using System.Diagnostics;
using System.Globalization;
using System.Numerics;

class Program
{
    // Grupo A: la fuerza bruta todavía entra. Grupo B: ya no entra; la dinámica contra el húngaro.
    // Si la fuerza bruta con el mayor tamaño del grupo A tarda menos de 0,5 s o más de 20 s, cambia TamA.
    static readonly int[] TamA = { 8, 9, 10, 11 };
    static readonly int[] TamB = { 16, 18, 20, 22 };
    const int Calentamientos = 1;
    const int Repeticiones = 3;
    const int Semilla = 2026;

    sealed record Contendiente(string Algoritmo, Func<Instancia, Resultado> Ejecutar);

    static readonly Contendiente FuerzaBrutaC = new("FuerzaBruta", Algoritmos.FuerzaBruta);
    static readonly Contendiente DinamicaC = new("DinamicaTabla", Algoritmos.DinamicaTabla);
    static readonly Contendiente HungaroC = new("Hungaro", Algoritmos.Hungaro);

    static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine(" H3: ¿VORAZ O DINAMICA? ASIGNACION DE PERSONAL");
        Console.WriteLine("--------------------------------------------------------");

        // ==================================
        // FASE 1: EL CONTRAEJEMPLO DEL VORAZ (a mano, 2×2)
        // ==================================
        Console.WriteLine("\nFASE 1: ¿ALCANZA ELEGIR LO MEJOR AHORA? (contraejemplo)");
        var chico = new Instancia(new double[,] { { 1, 2 }, { 2, 100 } });
        var vo = Algoritmos.Voraz(chico);
        Console.WriteLine($"Costos [[1,2],[2,100]]:  voraz = {vo.Costo:0.##}  |  fuerza bruta = {Algoritmos.FuerzaBruta(chico).Costo:0.##}  |  dinamica = {Algoritmos.DinamicaTabla(chico).Costo:0.##}  |  hungaro = {Algoritmos.Hungaro(chico).Costo:0.##}");
        Console.WriteLine("El voraz toma el par más barato (1) y queda obligado a pagar 100: no hay elección voraz segura.");

        // ==================================
        // FASE 2: VERIFICACION PREVIA (fuera de toda medicion)
        // ==================================
        Console.WriteLine("\nFASE 2: VERIFICACION (mismo costo y conteos exactos)");
        var conteos = new List<string> { "Algoritmo,Tamano,Operaciones,Referencia_teorica,Cociente,Costo_total" };
        var esperados = new Dictionary<int, double>();

        foreach (int n in TamA.Concat(TamB))
        {
            var ins = new Instancia(n, Semilla);
            var din = Algoritmos.DinamicaTabla(ins);
            var hun = Algoritmos.Hungaro(ins);

            if (din.Costo >= Instancia.Prohibido)
                throw new Exception($"Error: la instancia n = {n} no tiene asignacion factible.");
            if (Math.Abs(din.Costo - hun.Costo) > 1e-6)
                throw new Exception($"Error: dinamica y hungaro dan costos distintos (n = {n}).");

            double teoricoDin = (double)n * Math.Pow(2, n - 1);                 // n · 2^(n-1) transiciones
            if (din.Operaciones != (long)teoricoDin)
                throw new Exception($"Error: las transiciones de la dinamica no son n·2^(n-1) (n = {n}).");

            conteos.Add($"DinamicaTabla,{n},{din.Operaciones},{teoricoDin.ToString("F0", CultureInfo.InvariantCulture)},1.00,{din.Costo.ToString("F2", CultureInfo.InvariantCulture)}");
            conteos.Add($"Hungaro,{n},{hun.Operaciones},{((double)n * n * n).ToString("F0", CultureInfo.InvariantCulture)},{(hun.Operaciones / ((double)n * n * n)).ToString("F2", CultureInfo.InvariantCulture)},{hun.Costo.ToString("F2", CultureInfo.InvariantCulture)}");

            string nota = "dinamica = hungaro";

            if (n <= 11)
            {
                var fb = Algoritmos.FuerzaBruta(ins);
                if (Math.Abs(fb.Costo - din.Costo) > 1e-6)
                    throw new Exception($"Error: fuerza bruta y dinamica dan costos distintos (n = {n}).");
                if (fb.Operaciones != (long)LlamadasFuerzaBruta(n))
                    throw new Exception($"Error: las llamadas de la fuerza bruta no son la suma de n!/k! (n = {n}).");
                conteos.Add($"FuerzaBruta,{n},{fb.Operaciones},{LlamadasFuerzaBruta(n).ToString("F0", CultureInfo.InvariantCulture)},1.00,{fb.Costo.ToString("F2", CultureInfo.InvariantCulture)}");
                nota = "fuerza bruta = dinamica = hungaro";
            }

            if (n <= 16)
            {
                var memo = Algoritmos.DinamicaMemo(ins);
                if (Math.Abs(memo.Costo - din.Costo) > 1e-6)
                    throw new Exception($"Error: memoizacion y tabla dan costos distintos (n = {n}).");
                if (memo.Operaciones != din.Operaciones + 1)
                    throw new Exception($"Error: las llamadas con memoria no son 1 + n·2^(n-1) (n = {n}).");
                conteos.Add($"DinamicaMemo,{n},{memo.Operaciones},{(teoricoDin + 1).ToString("F0", CultureInfo.InvariantCulture)},1.00,{memo.Costo.ToString("F2", CultureInfo.InvariantCulture)}");
                nota += " = memoizacion";
            }

            esperados[n] = din.Costo;
            Console.WriteLine($"n = {n,2}: {nota}  (costo minimo {din.Costo:0.00}).");
        }

        // ==================================
        // FASE 3: LAS LLAMADAS «SIN MEMORIA» CONTRA «CON MEMORIA» (como Fibonacci en la clase 12)
        // ==================================
        Console.WriteLine("\nFASE 3: LLAMADAS SIN MEMORIA / CON MEMORIA (conteos exactos)");
        Console.WriteLine($"{"n",3} {"sin memoria (e·n!)",20} {"con memoria 1+n·2^(n-1)",24} {"cociente",12}");
        foreach (int n in TamA)
        {
            double sin = LlamadasFuerzaBruta(n);
            double con = 1 + (double)n * Math.Pow(2, n - 1);
            Console.WriteLine($"{n,3} {sin,20:N0} {con,24:N0} {sin / con,12:N0}");
        }

        // ==================================
        // FASE 4: MEDICIONES (1 calentamiento + 3 corridas medidas por algoritmo y tamaño)
        // ==================================
        Console.WriteLine("\nFASE 4: MEDICIONES");

        var resultados = new List<string> { "Algoritmo,Tamano,Repeticion,Tiempo_ms" };
        var resumen = new List<string> { "Algoritmo,Tamano,Promedio_ms,Factor_vs_fila_anterior,Min_ms,Max_ms,Dispersion_pct" };
        var promedios = new Dictionary<(string, int), double>();
        var factores = new Dictionary<(string, int), string>();
        var dispersiones = new Dictionary<(string, int), double>();

        Medir(TamA, 1, new[] { FuerzaBrutaC, DinamicaC }, esperados, resultados, resumen, promedios, factores, dispersiones);
        Medir(TamB, 2, new[] { DinamicaC, HungaroC }, esperados, resultados, resumen, promedios, factores, dispersiones);

        // ==================================
        // FASE 5: TABLAS EN PANTALLA
        // ==================================
        Console.WriteLine("\n----------------------------------------------------------------------");
        Console.WriteLine(" A) FUERZA BRUTA CONTRA DINAMICA (el mismo costo minimo en cada fila)");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("{0,3} {1,16} {2,8} {3,16} {4,8} {5,14}", "n", "FuerzaBruta ms", "FACTOR", "DinamicaTabla ms", "FACTOR", "FB / Dinamica");
        foreach (int n in TamA)
        {
            double tf = promedios[("FuerzaBruta", n)], td = promedios[("DinamicaTabla", n)];
            Console.WriteLine("{0,3} {1,16:F4} {2,8} {3,16:F4} {4,8} {5,14}", n, tf, factores[("FuerzaBruta", n)], td, factores[("DinamicaTabla", n)],
                td > 0 ? $"{tf / td:N0} veces" : "-");
        }

        Console.WriteLine("\n----------------------------------------------------------------------");
        Console.WriteLine(" B) DINAMICA CONTRA HUNGARO (la fuerza bruta ya no entra; +2 trabajadores por fila)");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("{0,3} {1,16} {2,8} {3,16} {4,8} {5,14}", "n", "DinamicaTabla ms", "FACTOR", "Hungaro ms", "FACTOR", "Dinamica / H.");
        foreach (int n in TamB)
        {
            double td = promedios[("DinamicaTabla", n)], th = promedios[("Hungaro", n)];
            Console.WriteLine("{0,3} {1,16:F4} {2,8} {3,16:F4} {4,8} {5,14}", n, td, factores[("DinamicaTabla", n)], th, factores[("Hungaro", n)],
                th > 0 ? $"{td / th:N0} veces" : "-");
        }

        // ==================================
        // FASE 6: CALIDAD DEL VORAZ EN 200 INSTANCIAS
        // ==================================
        Console.WriteLine("\nFASE 6: CALIDAD DEL VORAZ (200 instancias de n = 10, optimo = dinamica)");
        var voraz = new List<string> { "Semilla,Costo_Optimo,Costo_Voraz,Puestos_cubiertos_voraz,Sobrecosto_pct" };
        int incompletas = 0, noOptimas = 0, completas = 0;
        double sumaBrecha = 0, maxBrecha = 0;
        for (int semilla = 1; semilla <= 200; semilla++)
        {
            var ins = new Instancia(10, semilla);
            var opt = Algoritmos.DinamicaTabla(ins);
            var v = Algoritmos.Voraz(ins);
            if (v.Cubiertos < 10)
            {
                incompletas++;
                voraz.Add($"{semilla},{opt.Costo.ToString("F2", CultureInfo.InvariantCulture)},{v.Costo.ToString("F2", CultureInfo.InvariantCulture)},{v.Cubiertos},NA");
                continue;
            }
            completas++;
            double brecha = (v.Costo - opt.Costo) / opt.Costo * 100.0;
            if (brecha > 1e-9) noOptimas++;
            sumaBrecha += brecha;
            maxBrecha = Math.Max(maxBrecha, brecha);
            voraz.Add($"{semilla},{opt.Costo.ToString("F2", CultureInfo.InvariantCulture)},{v.Costo.ToString("F2", CultureInfo.InvariantCulture)},{v.Cubiertos},{brecha.ToString("F2", CultureInfo.InvariantCulture)}");
        }
        Console.WriteLine($"  voraz NO cubrio todos los puestos : {incompletas} de 200");
        Console.WriteLine($"  voraz cubrio todo pero NO optimo  : {noOptimas} de {completas} instancias completas");
        if (completas > 0)
            Console.WriteLine($"  sobrecosto medio del voraz        : {sumaBrecha / completas:0.00} %   (maximo {maxBrecha:0.00} %)");

        // ==================================
        // FASE 7: ARCHIVOS
        // ==================================
        string carpeta = Directory.GetCurrentDirectory();
        File.WriteAllLines(Path.Combine(carpeta, "resultados.csv"), resultados);
        File.WriteAllLines(Path.Combine(carpeta, "resumen.csv"), resumen);
        File.WriteAllLines(Path.Combine(carpeta, "conteos.csv"), conteos);
        File.WriteAllLines(Path.Combine(carpeta, "voraz.csv"), voraz);
        File.WriteAllLines(Path.Combine(carpeta, "tabla_medicion.md"), TablaMarkdown(promedios, factores, dispersiones));

        Console.WriteLine("\n======================================");
        Console.WriteLine(" EXPERIMENTO FINALIZADO");
        Console.WriteLine("======================================");
        Console.WriteLine("\nArchivos generados en " + carpeta + ":");
        Console.WriteLine("1. resultados.csv   (cada corrida)");
        Console.WriteLine("2. resumen.csv      (promedios, factor, minimo, maximo, dispersion)");
        Console.WriteLine("3. conteos.csv      (operaciones medidas contra teoricas)");
        Console.WriteLine("4. voraz.csv        (calidad del voraz en 200 instancias)");
        Console.WriteLine("5. tabla_medicion.md (tablas para pegar en comparacion.md)");
        Console.WriteLine("\nTodos los resultados fueron verificados.");
    }

    // Protocolo para un grupo de tamaños: 1 calentamiento + 3 corridas medidas + promedio.
    // «paso» = cuanto sube n entre filas (1 en el grupo A, 2 en el B); sirve para calcular el factor contra la fila anterior.
    static void Medir(int[] tamanos, int paso, Contendiente[] candidatos, Dictionary<int, double> esperados,
                      List<string> resultados, List<string> resumen,
                      Dictionary<(string, int), double> promedios, Dictionary<(string, int), string> factores,
                      Dictionary<(string, int), double> dispersiones)
    {
        foreach (int n in tamanos)
        {
            var ins = new Instancia(n, Semilla);
            Console.WriteLine($"\nTAMANO: n = {n} trabajadores y {n} puestos");

            foreach (var c in candidatos)
            {
                for (int w = 0; w < Calentamientos; w++) c.Ejecutar(ins);        // calentamiento (no se registra)

                double suma = 0, minimo = double.MaxValue, maximo = 0;
                for (int rep = 1; rep <= Repeticiones; rep++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    Stopwatch reloj = Stopwatch.StartNew();
                    var r = c.Ejecutar(ins);
                    reloj.Stop();

                    // Verificar fuera del tiempo medido
                    if (Math.Abs(r.Costo - esperados[n]) > 1e-6)
                        throw new Exception($"Error en {c.Algoritmo} (n = {n}): costo distinto del optimo.");

                    double tiempo = reloj.Elapsed.TotalMilliseconds;
                    suma += tiempo;
                    minimo = Math.Min(minimo, tiempo);
                    maximo = Math.Max(maximo, tiempo);

                    resultados.Add($"{c.Algoritmo},{n},{rep},{tiempo.ToString("F4", CultureInfo.InvariantCulture)}");
                    Console.WriteLine($"{c.Algoritmo,-14} - Ejecucion {rep}: {tiempo:F4} ms");
                }

                double promedio = suma / Repeticiones;
                double dispersion = promedio > 0 ? (maximo - minimo) / promedio * 100.0 : 0;
                promedios[(c.Algoritmo, n)] = promedio;
                dispersiones[(c.Algoritmo, n)] = dispersion;

                string factor = "NA";
                if (promedios.TryGetValue((c.Algoritmo, n - paso), out double anterior) && anterior > 0)
                    factor = (promedio / anterior).ToString("F2", CultureInfo.InvariantCulture);
                factores[(c.Algoritmo, n)] = factor;

                resumen.Add(
                    $"{c.Algoritmo},{n},{promedio.ToString("F4", CultureInfo.InvariantCulture)},{factor}," +
                    $"{minimo.ToString("F4", CultureInfo.InvariantCulture)},{maximo.ToString("F4", CultureInfo.InvariantCulture)}," +
                    $"{dispersion.ToString("F1", CultureInfo.InvariantCulture)}");

                Console.WriteLine($"PROMEDIO {c.Algoritmo}: {promedio:F4} ms   (dispersion entre corridas: {dispersion:F1} %)");
            }
        }
    }

    // Llamadas exactas de la fuerza bruta sin memoria: suma de n!/k! (k = 0..n) ≈ e·n!
    static double LlamadasFuerzaBruta(int n)
    {
        double sumaCuenta = 0;
        for (int k = 0; k <= n; k++)
        {
            double f = 1;
            for (int m = k + 1; m <= n; m++) f *= m;                              // n!/k!
            sumaCuenta += f;
        }
        return sumaCuenta;
    }

    static List<string> TablaMarkdown(
        Dictionary<(string, int), double> promedios,
        Dictionary<(string, int), string> factores,
        Dictionary<(string, int), double> dispersiones)
    {
        string Ms(double x) => x.ToString("F4", CultureInfo.InvariantCulture);
        string Fx(string a, int n) => factores[(a, n)] == "NA" ? "—" : "×" + factores[(a, n)];

        var md = new List<string>
        {
            "### A) Fuerza bruta contra dinámica (mismo costo mínimo; promedio de 3 corridas en ms; × = veces más que la fila anterior)",
            "",
            "| n | Fuerza bruta (ms) | × | Dinámica (ms) | × | Fuerza bruta / Dinámica | Dispersión FB | Dispersión Din. |",
            "|---|---|---|---|---|---|---|---|",
        };
        foreach (int n in TamA)
        {
            double tf = promedios[("FuerzaBruta", n)], td = promedios[("DinamicaTabla", n)];
            string veces = td > 0 ? (tf / td).ToString("N0", CultureInfo.InvariantCulture) + " veces" : "—";
            md.Add($"| {n} | {Ms(tf)} | {Fx("FuerzaBruta", n)} | {Ms(td)} | {Fx("DinamicaTabla", n)} | {veces} | {dispersiones[("FuerzaBruta", n)]:F1} % | {dispersiones[("DinamicaTabla", n)]:F1} % |");
        }

        md.Add("");
        md.Add("### B) Dinámica contra húngaro (la fuerza bruta ya no entra; n sube de 2 en 2)");
        md.Add("");
        md.Add("| n | Dinámica (ms) | × | Húngaro (ms) | × | Dinámica / Húngaro |");
        md.Add("|---|---|---|---|---|---|");
        foreach (int n in TamB)
        {
            double td = promedios[("DinamicaTabla", n)], th = promedios[("Hungaro", n)];
            string veces = th > 0 ? (td / th).ToString("N0", CultureInfo.InvariantCulture) + " veces" : "—";
            md.Add($"| {n} | {Ms(td)} | {Fx("DinamicaTabla", n)} | {Ms(th)} | {Fx("Hungaro", n)} | {veces} |");
        }
        return md;
    }
}

// =========================================================================================
// TIPOS
// =========================================================================================

sealed record Resultado(double Costo, long Operaciones, int Cubiertos = 0);

// INSTANCIA SINTÉTICA: matriz de costos trabajador x puesto con el modelo del proyecto
//   S = 0.50·S_tec + 0.25·S_exp + 0.15·S_disp + 0.10·S_carga     y     C = 100 - S
// Los pares que no cumplen las restricciones obligatorias cuestan «Prohibido» (un número enorme, no infinito:
// así la dinámica y la fuerza bruta siguen funcionando y un costo >= Prohibido significa «no factible»).
sealed class Instancia
{
    public const double Prohibido = 1_000_000;

    public int N { get; }
    public double[,] Costo { get; }
    public bool[,] Elegible { get; }

    public Instancia(double[,] costos)
    {
        N = costos.GetLength(0);
        Costo = costos;
        Elegible = new bool[N, N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                Elegible[i, j] = costos[i, j] < Prohibido;
    }

    public Instancia(int n, int semilla)
    {
        N = n;
        Costo = new double[n, n];
        Elegible = new bool[n, n];
        var rnd = new Random(semilla);

        var carga = new int[n];
        for (int i = 0; i < n; i++) carga[i] = rnd.Next(8, 17);                  // horas libres >= 24

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

        // Permutación oculta: garantiza que EXISTE al menos una asignación completa factible
        var garantizado = new int[n];
        for (int k = 0; k < n; k++) garantizado[k] = k;
        for (int k = n - 1; k > 0; k--)
        {
            int r = rnd.Next(k + 1);
            (garantizado[k], garantizado[r]) = (garantizado[r], garantizado[k]);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int nivel = rnd.Next(1, 6);
                int exp = rnd.Next(0, 13);
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
                }
                else
                {
                    Costo[i, j] = Prohibido;
                }
            }
        }
    }
}

static class Algoritmos
{
    // ================= FUERZA BRUTA: todas las permutaciones =================
    // Con m trabajadores libres hay m ramas: L(m) = m·L(m-1) + 1  ->  llamadas = suma de n!/k! ≈ e·n!
    private static long _llamadas;
    private static double _mejor;

    public static Resultado FuerzaBruta(Instancia ins)
    {
        _llamadas = 0;
        _mejor = double.PositiveInfinity;
        Probar(ins, 0, new bool[ins.N], 0.0);
        return new Resultado(_mejor, _llamadas);
    }

    private static void Probar(Instancia ins, int puesto, bool[] usado, double acumulado)
    {
        _llamadas++;
        if (puesto == ins.N)
        {
            if (acumulado < _mejor) _mejor = acumulado;
            return;
        }
        for (int i = 0; i < ins.N; i++)
        {
            if (usado[i]) continue;
            usado[i] = true;
            Probar(ins, puesto + 1, usado, acumulado + ins.Costo[i, puesto]);
            usado[i] = false;
        }
    }

    // ================= DINÁMICA, TABLA (abajo hacia arriba) =================
    // dp[m] = mejor costo de asignar a los primeros popcount(m) trabajadores EXACTAMENTE los puestos de la máscara m.
    // Subproblema = (cuántos trabajadores ya asignados, qué puestos ya usados): no importa en qué ORDEN se usaron.
    // Subestructura óptima: dp[m ∪ {j}] = min sobre j de dp[m] + costo[popcount(m), j].
    // Estados: 2^n.  Transiciones: n · 2^(n-1).  Memoria: 2^n números.
    public static Resultado DinamicaTabla(Instancia ins)
    {
        int n = ins.N;
        int total = 1 << n;
        var dp = new double[total];
        Array.Fill(dp, double.PositiveInfinity);
        dp[0] = 0;
        long operaciones = 0;

        for (int mascara = 0; mascara < total; mascara++)
        {
            int i = BitOperations.PopCount((uint)mascara);          // el trabajador que toca asignar
            if (i == n) continue;
            double actual = dp[mascara];

            for (int j = 0; j < n; j++)
            {
                if ((mascara & (1 << j)) != 0) continue;            // ese puesto ya está usado
                operaciones++;
                double candidato = actual + ins.Costo[i, j];
                int siguiente = mascara | (1 << j);
                if (candidato < dp[siguiente]) dp[siguiente] = candidato;
            }
        }
        return new Resultado(dp[total - 1], operaciones);
    }

    // ================= DINÁMICA, MEMOIZACIÓN (arriba hacia abajo) =================
    // Recursión normal, pero antes de calcular se mira la libreta (memo[mascara]); después de calcular, se anota.
    // Llamadas: 1 + n · 2^(n-1)   (contra ≈ e·n! sin memoria).
    public static Resultado DinamicaMemo(Instancia ins)
    {
        var memo = new double[1 << ins.N];
        Array.Fill(memo, double.NaN);
        long llamadas = 0;
        double costo = Memo(ins, 0, 0, memo, ref llamadas);
        return new Resultado(costo, llamadas);
    }

    private static double Memo(Instancia ins, int i, int mascara, double[] memo, ref long llamadas)
    {
        llamadas++;
        if (i == ins.N) return 0;
        if (!double.IsNaN(memo[mascara])) return memo[mascara];     // ya estaba en la libreta

        double mejor = double.PositiveInfinity;
        for (int j = 0; j < ins.N; j++)
        {
            if ((mascara & (1 << j)) != 0) continue;
            double c = ins.Costo[i, j] + Memo(ins, i + 1, mascara | (1 << j), memo, ref llamadas);
            if (c < mejor) mejor = c;
        }
        memo[mascara] = mejor;                                      // se anota
        return mejor;
    }

    // ================= VORAZ: el par más barato disponible AHORA =================
    public static Resultado Voraz(Instancia ins)
    {
        int n = ins.N;
        var pares = new List<(double c, int i, int j)>();
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (ins.Elegible[i, j]) pares.Add((ins.Costo[i, j], i, j));

        pares.Sort((x, y) =>
        {
            int r = x.c.CompareTo(y.c);
            if (r != 0) return r;
            r = x.i.CompareTo(y.i);
            return r != 0 ? r : x.j.CompareTo(y.j);
        });

        var trabajadorLibre = new bool[n];
        var puestoLibre = new bool[n];
        Array.Fill(trabajadorLibre, true);
        Array.Fill(puestoLibre, true);

        double costo = 0;
        int cubiertos = 0;
        foreach (var p in pares)
        {
            if (!trabajadorLibre[p.i] || !puestoLibre[p.j]) continue;
            trabajadorLibre[p.i] = false;
            puestoLibre[p.j] = false;
            costo += p.c;
            cubiertos++;
            if (cubiertos == n) break;
        }
        return new Resultado(costo, pares.Count, cubiertos);
    }

    // ================= HÚNGARO (referencia, O(n³), clase 6) =================
    public static Resultado Hungaro(Instancia ins)
    {
        int n = ins.N;
        var a = ins.Costo;
        var u = new double[n + 1];
        var v = new double[n + 1];
        var p = new int[n + 1];
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

        double costo = 0;
        for (int j = 1; j <= n; j++) costo += ins.Costo[p[j] - 1, j - 1];
        return new Resultado(costo, ops, n);
    }
}
