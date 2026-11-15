using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var trabajadores = CrearTrabajadores();

Console.WriteLine("PRÁCTICA C4 - BÚSQUEDA LINEAL CONTRA BÚSQUEDA BINARIA");
Console.WriteLine("Caso de estudio: asignación de personal técnico a proyectos eléctricos");
Console.WriteLine();

DemostrarBusqueda(trabajadores, 18);
MedirRendimiento(trabajadores);

static Trabajador[] CrearTrabajadores()
{
    // Registros de prueba basados en la estructura del proyecto.
    // Los códigos T01...T20 corresponden a los 20 trabajadores del caso de estudio.
    // El identificador numérico permite comparar búsquedas por una clave única.
    string[] perfiles =
    {
        "Responsable de proyectos",
        "Especialista MT",
        "Especialista MT",
        "Liniero MT",
        "Liniero MT",
        "Seguridad industrial",
        "Especialista instalaciones hospitalarias",
        "Instalador BT",
        "Instalador BT",
        "Técnico de pruebas eléctricas",
        "Supervisor de instalaciones",
        "Instalador BT",
        "Instalador BT",
        "Control de calidad",
        "Ingeniero de automatización",
        "Instrumentación industrial",
        "Técnico electromecánico",
        "Especialista MT",
        "Especialista en protecciones",
        "Técnico de mantenimiento"
    };

    var datos = new Trabajador[perfiles.Length];

    for (int i = 0; i < perfiles.Length; i++)
        datos[i] = new Trabajador(i + 1, $"T{i + 1:00}", perfiles[i]);

    // Para búsqueda binaria, el arreglo debe estar ordenado por Id.
    Array.Sort(datos, (a, b) => a.Id.CompareTo(b.Id));
    return datos;
}

static (int posicion, int pasos) BusquedaLineal(Trabajador[] datos, int idBuscado)
{
    int pasos = 0;

    for (int i = 0; i < datos.Length; i++)
    {
        pasos++;
        if (datos[i].Id == idBuscado)
            return (i, pasos);
    }

    return (-1, pasos);
}

static (int posicion, int pasos) BusquedaBinaria(Trabajador[] datos, int idBuscado)
{
    int pasos = 0;
    int inicio = 0;
    int fin = datos.Length - 1;

    while (inicio <= fin)
    {
        int medio = inicio + (fin - inicio) / 2;
        pasos++;

        if (datos[medio].Id == idBuscado)
            return (medio, pasos);

        if (datos[medio].Id < idBuscado)
            inicio = medio + 1;
        else
            fin = medio - 1;
    }

    return (-1, pasos);
}

static void DemostrarBusqueda(Trabajador[] trabajadores, int idBuscado)
{
    var lineal = BusquedaLineal(trabajadores, idBuscado);
    var binaria = BusquedaBinaria(trabajadores, idBuscado);

    Console.WriteLine("1. DEMOSTRACIÓN CON LOS 20 TRABAJADORES");
    Console.WriteLine($"Trabajador buscado: T{idBuscado:00}");
    Console.WriteLine($"Búsqueda lineal : posición {lineal.posicion}, pasos {lineal.pasos}");
    Console.WriteLine($"Búsqueda binaria: posición {binaria.posicion}, pasos {binaria.pasos}");

    if (lineal.posicion >= 0)
        Console.WriteLine($"Registro encontrado: {trabajadores[lineal.posicion].Codigo} - " +
                          $"{trabajadores[lineal.posicion].Perfil}");

    Console.WriteLine();
}

static void MedirRendimiento(Trabajador[] trabajadores)
{
    Console.WriteLine("2. MEDICIÓN DE TIEMPOS");
    Console.WriteLine("Datos: 20 trabajadores ordenados por identificador.");
    Console.WriteLine("Consultas: 1.000.000 búsquedas de identificadores existentes.");
    Console.WriteLine("Protocolo: 1 corrida de calentamiento + 3 corridas medidas + promedio.");
    Console.WriteLine();

    const int cantidadConsultas = 1_000_000;
    var random = new Random(21);
    var claves = new int[cantidadConsultas];

    // Las mismas claves se utilizan en ambos algoritmos para que la comparación sea justa.
    for (int i = 0; i < claves.Length; i++)
        claves[i] = random.Next(1, trabajadores.Length + 1);

    double MedirLineal()
    {
        var reloj = Stopwatch.StartNew();
        long acumulado = 0;

        foreach (int clave in claves)
            acumulado += BusquedaLineal(trabajadores, clave).posicion;

        reloj.Stop();
        GC.KeepAlive(acumulado);
        return reloj.Elapsed.TotalMilliseconds;
    }

    double MedirBinaria()
    {
        var reloj = Stopwatch.StartNew();
        long acumulado = 0;

        foreach (int clave in claves)
            acumulado += BusquedaBinaria(trabajadores, clave).posicion;

        reloj.Stop();
        GC.KeepAlive(acumulado);
        return reloj.Elapsed.TotalMilliseconds;
    }

    // Calentamiento: ejecuta ambos métodos antes de registrar tiempos.
    MedirLineal();
    MedirBinaria();

    double sumaLineal = 0;
    double sumaBinaria = 0;

    for (int corrida = 1; corrida <= 3; corrida++)
    {
        // Alternar el orden reduce el efecto de medir siempre un algoritmo primero.
        if (corrida % 2 == 1)
        {
            sumaLineal += MedirLineal();
            sumaBinaria += MedirBinaria();
        }
        else
        {
            sumaBinaria += MedirBinaria();
            sumaLineal += MedirLineal();
        }
    }

    double promedioLineal = sumaLineal / 3.0;
    double promedioBinaria = sumaBinaria / 3.0;
    double cociente = promedioBinaria > 0
        ? promedioLineal / promedioBinaria
        : 0;

    Console.WriteLine("Corridas medidas: 3");
    Console.WriteLine($"Promedio búsqueda lineal : {promedioLineal:F3} ms");
    Console.WriteLine($"Promedio búsqueda binaria: {promedioBinaria:F3} ms");
    Console.WriteLine($"Cociente lineal / binaria: {cociente:F2} veces");
    Console.WriteLine();

    Console.WriteLine("| Algoritmo | Tiempo promedio (ms) |");
    Console.WriteLine("|---|---:|");
    Console.WriteLine($"| Búsqueda lineal | {promedioLineal:F3} |");
    Console.WriteLine($"| Búsqueda binaria | {promedioBinaria:F3} |");
    Console.WriteLine($"| Cociente (lineal / binaria) | {cociente:F2} veces |");
    Console.WriteLine();

    Console.WriteLine("Nota: los tiempos dependen del equipo y de las condiciones de ejecución.");
    Console.WriteLine("El cociente indica cuántas veces el tiempo lineal representa al binario.");
}

record Trabajador(int Id, string Codigo, string Perfil);
