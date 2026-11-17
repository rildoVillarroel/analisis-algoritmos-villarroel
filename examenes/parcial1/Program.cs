// Pablo Rildo Villarroel Fernandez
// Registro de comprobantes — Analizar, curar y medir

using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Parcial1.VarianteB
{
    public static class Caja
    {
        // Solución original: compara cada elemento con todos los que siguen.
        // Peor caso: O(n²). No modifica el arreglo.
        public static bool HayComprobantesRepetidosPorPares(int[] comprobantes)
        {
            for (int a = 0; a < comprobantes.Length; a++)
                for (int b = a + 1; b < comprobantes.Length; b++)
                    if (comprobantes[a] == comprobantes[b])
                        return true;

            return false;
        }

        // Cura: HashSet permite detectar si un valor ya apareció.
        // Tiempo esperado O(n), espacio auxiliar O(n). No modifica el arreglo.
        public static bool HayComprobantesRepetidosCurado(int[] comprobantes)
        {
            var vistos = new HashSet<int>();

            foreach (int comprobante in comprobantes)
            {
                if (!vistos.Add(comprobante))
                    return true;
            }

            return false;
        }

        // Genera n enteros distintos y los mezcla para evitar que estén ordenados.
        public static int[] GenerarSinRepetidos(int n, int semilla = 77)
        {
            var datos = new int[n];

            for (int i = 0; i < n; i++)
                datos[i] = 1000 + i;

            var azar = new Random(semilla);

            for (int i = n - 1; i > 0; i--)
            {
                int k = azar.Next(i + 1);
                (datos[i], datos[k]) = (datos[k], datos[i]);
            }

            return datos;
        }
    }

    public static class Programa
    {
        private static readonly (int[] Datos, bool Esperado, string Nombre)[] Casos =
        {
            (new[] { 1010, 1003, 1009, 1003, 1005 }, true,  "repetido en el medio"),
            (new[] { 1010, 1003, 1009, 1004, 1005 }, false, "sin repetidos"),
            (Array.Empty<int>(), false, "lista vacía"),
            (new[] { 1008 }, false, "un solo elemento"),
            (new[] { 1004, 1001, 1002, 1006, 1004 }, true, "repetido en los extremos")
        };

        private sealed class Resultado
        {
            public int N { get; init; }
            public double[] ParesMs { get; init; } = Array.Empty<double>();
            public double[] CuraMs { get; init; } = Array.Empty<double>();
            public double PromedioPares => Promedio(ParesMs);
            public double PromedioCura => Promedio(CuraMs);
        }

        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("P1.2 — Registro de comprobantes");
            Console.WriteLine("Configuración recomendada: Release");
            Console.WriteLine();

            if (!ProbarCasos())
            {
                Console.WriteLine("ERROR: al menos un caso de prueba falló.");
                return;
            }

            Console.WriteLine("\nCalibrando el tamaño para que el algoritmo por pares tarde aproximadamente 2 segundos...");
            int nMayor = CalibrarTamanoMayor();
            int nBase = Math.Max(1, nMayor / 8);
            nMayor = nBase * 8;

            int[] tamanos = { nBase, nBase * 2, nBase * 4, nMayor };
            var resultados = new List<Resultado>();

            Console.WriteLine($"\nTamaños seleccionados: {tamanos[0]}, {tamanos[1]}, {tamanos[2]}, {tamanos[3]}");
            Console.WriteLine("Cada algoritmo: 1 calentamiento + 3 mediciones.");
            Console.WriteLine();

            foreach (int n in tamanos)
            {
                Console.WriteLine($"Midiendo n = {n:N0} ...");
                int[] datos = Caja.GenerarSinRepetidos(n, 77);

                // Calentamiento no contabilizado: activa JIT y prepara el código.
                _ = Caja.HayComprobantesRepetidosPorPares(datos);
                _ = Caja.HayComprobantesRepetidosCurado(datos);

                var pares = new double[3];
                var cura = new double[3];

                for (int r = 0; r < 3; r++)
                {
                    pares[r] = Medir(() => Caja.HayComprobantesRepetidosPorPares(datos));
                    cura[r] = Medir(() => Caja.HayComprobantesRepetidosCurado(datos));
                }

                resultados.Add(new Resultado { N = n, ParesMs = pares, CuraMs = cura });
            }

            ImprimirTabla(resultados);
            GuardarMedicionMarkdown(resultados);
        }

        private static bool ProbarCasos()
        {
            Console.WriteLine("VERIFICACIÓN DE LOS CINCO CASOS");
            bool todosCorrectos = true;

            foreach (var caso in Casos)
            {
                bool lenta = Caja.HayComprobantesRepetidosPorPares(caso.Datos);
                bool curada = Caja.HayComprobantesRepetidosCurado(caso.Datos);
                bool correcto = lenta == caso.Esperado && curada == caso.Esperado;
                todosCorrectos &= correcto;

                Console.WriteLine(
                    $"{caso.Nombre,-25} esperado={caso.Esperado,-5} " +
                    $"por pares={lenta,-5} curado={curada,-5} " +
                    $"{(correcto ? "OK" : "ERROR")}");
            }

            return todosCorrectos;
        }

        private static int CalibrarTamanoMayor()
        {
            const double objetivoSegundos = 2.0;
            int candidato = 4000;

            // Ajusta el tamaño por la raíz cuadrada de la razón de tiempos,
            // porque el algoritmo por pares tiene crecimiento cuadrático.
            for (int intento = 0; intento < 6; intento++)
            {
                int[] datos = Caja.GenerarSinRepetidos(candidato, 77);
                _ = Caja.HayComprobantesRepetidosPorPares(datos); // calentamiento
                double segundos = Medir(() =>
                    Caja.HayComprobantesRepetidosPorPares(datos)) / 1000.0;

                Console.WriteLine($"  prueba n={candidato:N0}: {segundos:F3} s");

                if (segundos >= 0.8 && segundos <= 8.0)
                    return candidato;

                if (segundos <= 0.001)
                    candidato = Math.Min(candidato * 8, 2_000_000);
                else
                {
                    double factor = Math.Sqrt(objetivoSegundos / segundos);
                    factor = Math.Clamp(factor, 0.5, 4.0);
                    long nuevo = (long)Math.Round(candidato * factor);
                    candidato = (int)Math.Clamp(nuevo, 1000, 2_000_000);
                }
            }

            return candidato;
        }

        private static double Medir(Func<bool> accion)
        {
            var reloj = Stopwatch.StartNew();
            bool resultado = accion();
            reloj.Stop();

            // En la lista de medición no hay repetidos: el resultado debe ser false.
            if (resultado)
                throw new InvalidOperationException("La lista de medición debía estar libre de repetidos.");

            return reloj.Elapsed.TotalMilliseconds;
        }

        private static double Promedio(double[] valores)
        {
            double suma = 0;
            foreach (double valor in valores) suma += valor;
            return suma / valores.Length;
        }

        private static double Cociente(double actual, double anterior)
        {
            return anterior <= 0 ? double.NaN : actual / anterior;
        }

        private static void ImprimirTabla(List<Resultado> resultados)
        {
            Console.WriteLine("\nRESULTADOS — PROMEDIO DE TRES CORRIDAS (ms)");
            Console.WriteLine(
                $"{"n",12} | {"Pares ms",12} | {"Veces",8} | {"Curado ms",12} | {"Veces",8}");
            Console.WriteLine(new string('-', 66));

            for (int i = 0; i < resultados.Count; i++)
            {
                var r = resultados[i];
                string ratioPares = i == 0 ? "—" :
                    Cociente(r.PromedioPares, resultados[i - 1].PromedioPares).ToString("F2", CultureInfo.InvariantCulture);
                string ratioCura = i == 0 ? "—" :
                    Cociente(r.PromedioCura, resultados[i - 1].PromedioCura).ToString("F2", CultureInfo.InvariantCulture);

                Console.WriteLine(
                    $"{r.N,12:N0} | {r.PromedioPares,12:F3} | {ratioPares,8} | " +
                    $"{r.PromedioCura,12:F3} | {ratioCura,8}");
            }

            var peor = resultados.OrderByDescending(r => r.PromedioPares).First();
            Console.WriteLine($"\nPeor tiempo medido del algoritmo por pares: {peor.PromedioPares:F3} ms con n={peor.N:N0}.");
        }

        private static void GuardarMedicionMarkdown(List<Resultado> resultados)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Rildo Villarroel");
            sb.AppendLine();
            sb.AppendLine("# P1.2 — Curar y medir");
            sb.AppendLine();
            sb.AppendLine("## 1. Cura");
            sb.AppendLine();
            sb.AppendLine("La función `HayComprobantesRepetidosCurado` utiliza `HashSet<int>` para registrar los valores ya vistos. Devuelve `true` cuando encuentra uno que ya estaba registrado y `false` si termina el recorrido.");
            sb.AppendLine();
            sb.AppendLine("- **Tiempo esperado:** `O(n)`.");
            sb.AppendLine("- **Espacio auxiliar:** `O(n)`.");
            sb.AppendLine("- **No modifica** el arreglo recibido.");
            sb.AppendLine("- Los cinco casos de prueba se verificaron antes de medir.");
            sb.AppendLine();
            sb.AppendLine("## 2. Protocolo");
            sb.AppendLine();
            sb.AppendLine("Medición realizada en configuración Release/equivalente. Para cada tamaño y cada función se ejecutó una corrida de calentamiento y tres corridas medidas. Se informa el promedio. El peor caso utilizado es una lista sin repetidos.");
            sb.AppendLine();
            sb.AppendLine("## 3. Tabla de resultados");
            sb.AppendLine();
            sb.AppendLine("| n | Por pares promedio (ms) | Veces vs. fila anterior | Curado promedio (ms) | Veces vs. fila anterior |");
            sb.AppendLine("|---:|---:|---:|---:|---:|");

            for (int i = 0; i < resultados.Count; i++)
            {
                var r = resultados[i];
                string rp = i == 0 ? "—" : Cociente(r.PromedioPares, resultados[i - 1].PromedioPares).ToString("F2", CultureInfo.InvariantCulture);
                string rc = i == 0 ? "—" : Cociente(r.PromedioCura, resultados[i - 1].PromedioCura).ToString("F2", CultureInfo.InvariantCulture);

                sb.AppendLine($"| {r.N} | {r.PromedioPares:F3} | {rp} | {r.PromedioCura:F3} | {rc} |");
            }

            var peor = resultados.OrderByDescending(r => r.PromedioPares).First();
            sb.AppendLine();
            sb.AppendLine("## 4. Conclusión");
            sb.AppendLine();
            sb.AppendLine($"El peor tiempo promedio medido para la función por pares fue **{peor.PromedioPares:F3} ms**, con **n = {peor.N:N0}**.");
            sb.AppendLine();
            sb.AppendLine("La función por pares tiene costo teórico `O(n²)`: al duplicar el tamaño se espera un cociente cercano a 4. La cura tiene costo esperado `O(n)`: se espera un cociente cercano a 2. Las diferencias respecto de esos valores pueden deberse a JIT, caché, recolección de basura, planificación del sistema operativo y resolución del cronómetro. Se deben interpretar tendencias, no exigir cocientes exactos.");
            sb.AppendLine();
            sb.AppendLine("Los tiempos corresponden al equipo donde se ejecutó este programa; pueden variar en otro equipo.");
            File.WriteAllText("medicion.md", sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine("\nArchivo medicion.md generado/actualizado con los tiempos reales de esta ejecución.");
        }
    }
}
