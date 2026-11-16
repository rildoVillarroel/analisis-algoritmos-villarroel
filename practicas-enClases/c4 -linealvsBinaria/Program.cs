// COMPARACIÓN MEDIDA: BÚSQUEDA LINEAL CONTRA BÚSQUEDA BINARIA
//
// Datos: arreglo ORDENADO de enteros (1.000.000 en el tamaño principal) y 1.000 claves que SÍ existen en él.
// Datos sintéticos generados con semilla fija.
//
// PROTOCOLO:
//   · 3 calentamientos por algoritmo
//   · 3 mediciones por algoritmo y tamaño
//   · Promedio = suma de tiempos / 3
//   · Verificación EstaOrdenado() + cada clave encontrada en su posición correcta (fuera del tiempo medido)
//   · Resultados en archivos CSV
//
// CSV que genera:
//   resultados.csv -> cada corrida   (Algoritmo,Tamano,Repeticion,Tiempo_ms)
//   resumen.csv    -> promedios      (Algoritmo,Tamano,Promedio_ms)
//   conteos.csv    -> comparaciones exactas (no dependen de la máquina)
//
// Ejecutar:  dotnet run -c Release

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

class Program
{
    static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        int[] tamanos = { 100000, 500000, 1000000 };   // el tamaño pedido es 1.000.000; los otros muestran la tendencia
        int cantidadClaves = 1000;
        int calentamientos = 3;
        int repeticiones = 3;
        string[] algoritmos = { "BusquedaLineal", "BusquedaBinaria" };

        Random random = new Random(12345);

        Console.WriteLine("----------------------------------------------");
        Console.WriteLine(" BUSQUEDA LINEAL CONTRA BUSQUEDA BINARIA");
        Console.WriteLine("----------------------------------------------");

        // Generar los datos originales (arreglo ordenado + claves que existen + posición esperada de cada clave)
        var arreglos = new Dictionary<int, int[]>();
        var claves = new Dictionary<int, int[]>();
        var esperados = new Dictionary<int, int[]>();
        foreach (int n in tamanos)
        {
            int[] arreglo = GenerarOrdenado(n, random);
            GenerarClaves(arreglo, cantidadClaves, random, out int[] k, out int[] pos);
            arreglos[n] = arreglo;
            claves[n] = k;
            esperados[n] = pos;
        }

        // ==================================
        // FASE 1: CALENTAMIENTO (3 por algoritmo)
        // ==================================

        Console.WriteLine("\nFASE 1: CALENTAMIENTO");

        int[] arregloCalentamiento = GenerarOrdenado(10000, random);
        GenerarClaves(arregloCalentamiento, 100, random, out int[] clavesCalentamiento, out _);

        foreach (string algoritmo in algoritmos)
        {
            for (int i = 0; i < calentamientos; i++)
            {
                EjecutarTodas(algoritmo, arregloCalentamiento, clavesCalentamiento);
                Console.WriteLine($"{algoritmo} - Calentamiento {i + 1} completado.");
            }
        }

        // ==================================
        // FASE 2: VERIFICACION PREVIA
        // ==================================

        Console.WriteLine("\nFASE 2: VERIFICACION");

        foreach (int n in tamanos)
        {
            if (!EstaOrdenado(arreglos[n]))
                throw new Exception($"Error: el arreglo de {n} elementos NO esta ordenado.");

            foreach (string algoritmo in algoritmos)
                VerificarBusquedas(algoritmo, arreglos[n], claves[n], esperados[n]);

            Console.WriteLine(
                $"{n:N0} elementos: arreglo ordenado y las {cantidadClaves:N0} claves se encuentran con ambos algoritmos."
            );
        }

        // ==================================
        // FASE 3: MEDICIONES
        // ==================================

        Console.WriteLine("\nFASE 3: MEDICIONES");

        var resultados = new List<string>();
        resultados.Add("Algoritmo,Tamano,Repeticion,Tiempo_ms");

        var resumen = new List<string>();
        resumen.Add("Algoritmo,Tamano,Promedio_ms");

        var conteos = new List<string>();
        conteos.Add("Algoritmo,Tamano,Claves,Comparaciones_totales,Comparaciones_por_clave");

        var tablaPromedios = new List<(string algoritmo, int tamano, double promedio, long comparaciones)>();

        foreach (int n in tamanos)
        {
            Console.WriteLine($"\nTAMANO: {n:N0} elementos, {cantidadClaves:N0} claves");

            foreach (string algoritmo in algoritmos)
            {
                double suma = 0;

                for (int r = 1; r <= repeticiones; r++)
                {
                    // Iniciar cronometro
                    Stopwatch reloj = Stopwatch.StartNew();

                    long suma_posiciones = EjecutarTodas(algoritmo, arreglos[n], claves[n]);

                    reloj.Stop();

                    // Verificar fuera del tiempo medido
                    if (suma_posiciones != SumaPosiciones(esperados[n]))
                        throw new Exception($"Error en {algoritmo}: encontro posiciones incorrectas.");

                    double tiempo = reloj.Elapsed.TotalMilliseconds;
                    suma += tiempo;

                    resultados.Add(
                        $"{algoritmo},{n},{r}," +
                        tiempo.ToString("F4", CultureInfo.InvariantCulture)
                    );

                    Console.WriteLine($"{algoritmo} - Ejecucion {r}: {tiempo:F4} ms");
                }

                double promedio = suma / repeticiones;

                resumen.Add(
                    $"{algoritmo},{n}," +
                    promedio.ToString("F4", CultureInfo.InvariantCulture)
                );

                // Comparaciones exactas (pasada aparte, fuera del tiempo medido)
                long comparaciones = ContarComparaciones(algoritmo, arreglos[n], claves[n]);
                double porClave = (double)comparaciones / cantidadClaves;

                conteos.Add(
                    $"{algoritmo},{n},{cantidadClaves},{comparaciones}," +
                    porClave.ToString("F2", CultureInfo.InvariantCulture)
                );

                tablaPromedios.Add((algoritmo, n, promedio, comparaciones));

                Console.WriteLine($"PROMEDIO {algoritmo}: {promedio:F4} ms");
            }
        }

        // ==================================
        // FASE 4: MOSTRAR TABLA FINAL
        // ==================================

        Console.WriteLine("\n---------------------------------------------------------------");
        Console.WriteLine(" TABLA DE PROMEDIOS");
        Console.WriteLine("---------------------------------------------------------------");

        Console.WriteLine(
            "{0,-16} {1,-12} {2,-15} {3,-18}",
            "ALGORITMO", "ELEMENTOS", "PROMEDIO (ms)", "COMPARACIONES"
        );

        Console.WriteLine("---------------------------------------------------------------");

        foreach (var fila in tablaPromedios)
        {
            Console.WriteLine(
                "{0,-16} {1,-12} {2,-15:F4} {3,-18:N0}",
                fila.algoritmo, fila.tamano.ToString("N0"), fila.promedio, fila.comparaciones
            );
        }

        Console.WriteLine("\nCUANTAS VECES MAS RAPIDA ES LA BINARIA (tiempo lineal / tiempo binaria):");
        foreach (int n in tamanos)
        {
            double lineal = 0, binaria = 0;
            foreach (var fila in tablaPromedios)
            {
                if (fila.tamano != n) continue;
                if (fila.algoritmo == "BusquedaLineal") lineal = fila.promedio;
                else binaria = fila.promedio;
            }
            string veces = binaria > 0 ? $"{lineal / binaria:N0}" : "(tiempo binaria demasiado pequeno)";
            Console.WriteLine($"  n = {n,9:N0}: {veces} veces");
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
        Console.WriteLine(" EXPERIMENTO FINALIZADO");
        Console.WriteLine("======================================");

        Console.WriteLine("\nArchivos generados:");
        Console.WriteLine($"1. {rutaResultados}");
        Console.WriteLine($"2. {rutaResumen}");
        Console.WriteLine($"3. {rutaConteos}");

        Console.WriteLine("\nTodos los resultados fueron verificados.");
    }

    // ==================================
    // GENERAR DATOS
    // ==================================

    // Arreglo ORDENADO y sin repetidos: cada elemento es el anterior + un salto aleatorio entre 1 y 5
    static int[] GenerarOrdenado(int n, Random random)
    {
        int[] arreglo = new int[n];
        int valor = 0;

        for (int i = 0; i < n; i++)
        {
            valor += random.Next(1, 6);
            arreglo[i] = valor;
        }

        return arreglo;
    }

    // Claves tomadas DEL arreglo (todas existen), junto con la posicion donde estan
    static void GenerarClaves(int[] arreglo, int cantidad, Random random, out int[] claves, out int[] posiciones)
    {
        claves = new int[cantidad];
        posiciones = new int[cantidad];

        for (int k = 0; k < cantidad; k++)
        {
            int pos = random.Next(0, arreglo.Length);
            posiciones[k] = pos;
            claves[k] = arreglo[pos];
        }
    }

    // ==================================
    // ALGORITMO 1: BUSQUEDA LINEAL O(n)
    // ==================================

    static int BusquedaLineal(int[] arreglo, int clave)
    {
        for (int i = 0; i < arreglo.Length; i++)
        {
            if (arreglo[i] == clave)
                return i;
        }

        return -1;
    }

    // ==================================
    // ALGORITMO 2: BUSQUEDA BINARIA O(log n)
    // ==================================

    static int BusquedaBinaria(int[] arreglo, int clave)
    {
        int izquierda = 0;
        int derecha = arreglo.Length - 1;

        while (izquierda <= derecha)
        {
            int medio = izquierda + (derecha - izquierda) / 2;

            if (arreglo[medio] == clave)
                return medio;

            if (arreglo[medio] < clave)
                izquierda = medio + 1;
            else
                derecha = medio - 1;
        }

        return -1;
    }

    // ==================================
    // EJECUTAR TODAS LAS CLAVES CON UN ALGORITMO
    // Devuelve la suma de las posiciones halladas (evita que el compilador descarte el trabajo
    // y sirve para verificar fuera del tiempo medido)
    // ==================================

    static long EjecutarTodas(string algoritmo, int[] arreglo, int[] claves)
    {
        long suma = 0;

        if (algoritmo == "BusquedaLineal")
        {
            for (int k = 0; k < claves.Length; k++)
                suma += BusquedaLineal(arreglo, claves[k]);
        }
        else
        {
            for (int k = 0; k < claves.Length; k++)
                suma += BusquedaBinaria(arreglo, claves[k]);
        }

        return suma;
    }

    static long SumaPosiciones(int[] posiciones)
    {
        long suma = 0;
        foreach (int p in posiciones) suma += p;
        return suma;
    }

    // ==================================
    // CONTAR COMPARACIONES (pasada aparte, no se cronometra)
    // ==================================

    static long ContarComparaciones(string algoritmo, int[] arreglo, int[] claves)
    {
        long total = 0;

        foreach (int clave in claves)
        {
            if (algoritmo == "BusquedaLineal")
            {
                for (int i = 0; i < arreglo.Length; i++)
                {
                    total++;
                    if (arreglo[i] == clave) break;
                }
            }
            else
            {
                int izquierda = 0;
                int derecha = arreglo.Length - 1;

                while (izquierda <= derecha)
                {
                    int medio = izquierda + (derecha - izquierda) / 2;
                    total++;

                    if (arreglo[medio] == clave) break;

                    if (arreglo[medio] < clave) izquierda = medio + 1;
                    else derecha = medio - 1;
                }
            }
        }

        return total;
    }

    // ==================================
    // VERIFICAR QUE CADA CLAVE SE ENCUENTRA EN SU POSICION
    // ==================================

    static void VerificarBusquedas(string algoritmo, int[] arreglo, int[] claves, int[] esperados)
    {
        for (int k = 0; k < claves.Length; k++)
        {
            int pos = algoritmo == "BusquedaLineal"
                ? BusquedaLineal(arreglo, claves[k])
                : BusquedaBinaria(arreglo, claves[k]);

            if (pos != esperados[k])
                throw new Exception($"Error: {algoritmo} no encontro la clave {claves[k]} en su posicion.");
        }
    }

    // ==================================
    // VERIFICAR SI EL ARREGLO ESTA ORDENADO
    // ==================================

    static bool EstaOrdenado(int[] arreglo)
    {
        for (int i = 0; i < arreglo.Length - 1; i++)
        {
            if (arreglo[i] > arreglo[i + 1])
                return false;
        }

        return true;
    }
}
