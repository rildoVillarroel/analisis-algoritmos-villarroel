
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int[] tamanos = { 10000, 20000, 40000, 80000 };

        int calentamientos = 3;
        int repeticiones = 3;

        Random random = new Random(12345);

        Console.WriteLine("--------------------------------------");
        Console.WriteLine(" TORNEO DE ALGORITMOS DE ORDENAMIENTO");
        Console.WriteLine("--------------------------------------");

        // Generar los datos originales
        var datos = tamanos.ToDictionary(
            n => n,
            n => GenerarArreglo(n, random)
        );

        // ==================================
        // FASE 1: CALENTAMIENTO
        // ==================================

        Console.WriteLine("\nFASE 1: CALENTAMIENTO");

        int[] datosCalentamiento = GenerarArreglo(2000, random);

        for (int i = 0; i < calentamientos; i++)
        {
            int[] copia = (int[])datosCalentamiento.Clone();
            BurbujaSort(copia);

            copia = (int[])datosCalentamiento.Clone();
            MergeSort(copia);

            Console.WriteLine($"Calentamiento {i + 1} completado.");
        }

        // ==================================
        // FASE 2: VERIFICACION PREVIA
        // ==================================

        Console.WriteLine("\nFASE 2: VERIFICACION");

        foreach (int n in tamanos)
        {
            int[] copiaBurbuja = (int[])datos[n].Clone();
            BurbujaSort(copiaBurbuja);

            int[] copiaMerge = (int[])datos[n].Clone();
            MergeSort(copiaMerge);

            if (!EstaOrdenado(copiaBurbuja) ||
                !EstaOrdenado(copiaMerge))
            {
                throw new Exception(
                    "Error: un algoritmo no ordeno correctamente."
                );
            }

            Console.WriteLine(
                $"{n} elementos: ambos algoritmos correctos."
            );
        }

        // ==================================
        // FASE 3: MEDICIONES
        // ==================================

        Console.WriteLine("\nFASE 3: MEDICIONES");

        // Archivo con las mediciones individuales
        var resultados = new List<string>();

        resultados.Add(
            "Algoritmo,Tamano,Repeticion,Tiempo_ms"
        );

        // Archivo con los promedios
        var resumen = new List<string>();

        resumen.Add(
            "Algoritmo,Tamano,Promedio_ms"
        );

        // Guardar los promedios para mostrarlos al final
        var tablaPromedios = new List<(string algoritmo,
                                       int tamano,
                                       double promedio)>();

        foreach (int n in tamanos)
        {
            Console.WriteLine($"\nTAMANO: {n:N0} elementos");

            foreach (string algoritmo in new[] {
                "BurbujaSort", "MergeSort"
            })
            {
                double suma = 0;

                for (int r = 1; r <= repeticiones; r++)
                {
                    int[] copia = (int[])datos[n].Clone();

                    // Iniciar cronometro
                    Stopwatch reloj = Stopwatch.StartNew();

                    if (algoritmo == "BurbujaSort")
                        BurbujaSort(copia);
                    else
                        MergeSort(copia);

                    reloj.Stop();

                    // Verificar fuera del tiempo medido
                    if (!EstaOrdenado(copia))
                    {
                        throw new Exception(
                            $"Error en {algoritmo}"
                        );
                    }

                    double tiempo = reloj.Elapsed.TotalMilliseconds;

                    suma += tiempo;

                    // Guardar cada ejecucion
                    resultados.Add(
                        $"{algoritmo},{n},{r}," +
                        tiempo.ToString(
                            "F4",
                            CultureInfo.InvariantCulture
                        )
                    );

                    Console.WriteLine(
                        $"{algoritmo} - Ejecucion {r}: " +
                        $"{tiempo:F4} ms"
                    );
                }

                // Calcular promedio
                double promedio = suma / repeticiones;

                // Guardar promedio en el archivo resumen
                resumen.Add(
                    $"{algoritmo},{n}," +
                    promedio.ToString(
                        "F4",
                        CultureInfo.InvariantCulture
                    )
                );

                // Guardar para mostrar en pantalla
                tablaPromedios.Add(
                    (algoritmo, n, promedio)
                );

                Console.WriteLine(
                    $"PROMEDIO {algoritmo}: {promedio:F4} ms"
                );
            }
        }

        // ==================================
        // FASE 4: MOSTRAR TABLA FINAL
        // ==================================

        Console.WriteLine("\n----------------------------------------");
        Console.WriteLine(" TABLA DE PROMEDIOS");
        Console.WriteLine("----------------------------------------");

        Console.WriteLine(
            "{0,-15} {1,-15} {2,-15}",
            "ALGORITMO",
            "ELEMENTOS",
            "PROMEDIO (ms)"
        );

        Console.WriteLine("-----------------------------------------------");

        foreach (var fila in tablaPromedios)
        {
            Console.WriteLine(
                "{0,-15} {1,-15} {2,-15:F4}",
                fila.algoritmo,
                fila.tamano,
                fila.promedio
            );
        }

        // ==================================
        // FASE 5: EXPORTAR ARCHIVOS CSV
        // ==================================

        string rutaResultados = Path.Combine(
            Directory.GetCurrentDirectory(),
            "resultados.csv"
        );

        string rutaResumen = Path.Combine(
            Directory.GetCurrentDirectory(),
            "resumen.csv"
        );

        File.WriteAllLines(rutaResultados, resultados);
        File.WriteAllLines(rutaResumen, resumen);

        Console.WriteLine("\n======================================");
        Console.WriteLine(" EXPERIMENTO FINALIZADO");
        Console.WriteLine("======================================");

        Console.WriteLine("\nArchivos generados:");

        Console.WriteLine($"1. {rutaResultados}");
        Console.WriteLine($"2. {rutaResumen}");

        Console.WriteLine("\nTodos los algoritmos fueron verificados.");
    }

    // ==================================
    // GENERAR NUMEROS ALEATORIOS
    // ==================================

    static int[] GenerarArreglo(int n, Random random)
    {
        int[] arreglo = new int[n];

        for (int i = 0; i < n; i++)
            arreglo[i] = random.Next(0, 1000000);

        return arreglo;
    }

    // ==================================
    // ALGORITMO 1: BUBBLE SORT O(n^2)
    // ==================================

    static void BurbujaSort(int[] arreglo)
    {
        int n = arreglo.Length;

        for (int i = 0; i < n - 1; i++)
        {
            bool intercambio = false;

            for (int j = 0; j < n - i - 1; j++)
            {
                if (arreglo[j] > arreglo[j + 1])
                {
                    int temporal = arreglo[j];
                    arreglo[j] = arreglo[j + 1];
                    arreglo[j + 1] = temporal;

                    intercambio = true;
                }
            }

            if (!intercambio)
                break;
        }
    }

    // ==================================
    // ALGORITMO 2: MERGE SORT O(n log n)
    // ==================================

    static void MergeSort(int[] arreglo)
    {
        if (arreglo.Length <= 1)
            return;

        int[] auxiliar = new int[arreglo.Length];

        Dividir(arreglo, auxiliar, 0, arreglo.Length - 1);
    }

    static void Dividir(
        int[] arreglo,
        int[] auxiliar,
        int izquierda,
        int derecha)
    {
        if (izquierda >= derecha)
            return;

        int medio = izquierda + (derecha - izquierda) / 2;

        Dividir(arreglo, auxiliar, izquierda, medio);
        Dividir(arreglo, auxiliar, medio + 1, derecha);

        Mezclar(arreglo, auxiliar, izquierda, medio, derecha);
    }

    static void Mezclar(
        int[] arreglo,
        int[] auxiliar,
        int izquierda,
        int medio,
        int derecha)
    {
        int i = izquierda;
        int j = medio + 1;
        int k = izquierda;

        while (i <= medio && j <= derecha)
        {
            if (arreglo[i] <= arreglo[j])
                auxiliar[k++] = arreglo[i++];
            else
                auxiliar[k++] = arreglo[j++];
        }

        while (i <= medio)
            auxiliar[k++] = arreglo[i++];

        while (j <= derecha)
            auxiliar[k++] = arreglo[j++];

        for (int x = izquierda; x <= derecha; x++)
            arreglo[x] = auxiliar[x];
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
