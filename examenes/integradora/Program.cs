using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Integradora.VarianteB;

public sealed record Util(string Codigo, int CostoBs, int Utilidad);
public interface IComprador
{
    (int utilidad, List<string> codigos) Elegir(Util[] utiles, int presupuestoBs);
    bool EstaEnLaCompra(string codigo);
}

public sealed class CompradorIngenuo : IComprador
{
    private List<string> _elegidos = new();
    public (int utilidad, List<string> codigos) Elegir(Util[] utiles, int presupuestoBs)
    {
        ArgumentNullException.ThrowIfNull(utiles);
        if (presupuestoBs < 0) throw new ArgumentOutOfRangeException(nameof(presupuestoBs));
        if (utiles.Length >= 63) throw new ArgumentException("La máscara long no admite 63 o más útiles.");
        int n = utiles.Length, mejor = 0;
        long mejorMascara = 0, limite = 1L << n;
        for (long mascara = 0; mascara < limite; mascara++)
        {
            int costo = 0, utilidad = 0;
            for (int i = 0; i < n; i++)
                if ((mascara & (1L << i)) != 0)
                { costo += utiles[i].CostoBs; utilidad += utiles[i].Utilidad; }
            if (costo <= presupuestoBs && utilidad > mejor)
            { mejor = utilidad; mejorMascara = mascara; }
        }
        _elegidos = new();
        for (int i = 0; i < n; i++)
            if ((mejorMascara & (1L << i)) != 0) _elegidos.Add(utiles[i].Codigo);
        return (mejor, new List<string>(_elegidos));
    }
    public bool EstaEnLaCompra(string codigo) => _elegidos.Contains(codigo);
}

// Mochila 0/1: tabla DP y reconstrucción; HashSet para pertenencia O(1) promedio.
public sealed class CompradorMejorado : IComprador
{
    private HashSet<string> _codigosElegidos = new(StringComparer.Ordinal);

    public (int utilidad, List<string> codigos) Elegir(Util[] utiles, int presupuestoBs)
    {
        ArgumentNullException.ThrowIfNull(utiles);
        if (presupuestoBs < 0) throw new ArgumentOutOfRangeException(nameof(presupuestoBs));
        foreach (var x in utiles)
        {
            if (x.CostoBs < 0 || x.Utilidad < 0)
                throw new ArgumentException("Costo y utilidad deben ser no negativos.");
        }
        int n = utiles.Length;
        _codigosElegidos = new(StringComparer.Ordinal);
        if (n == 0) return (0, new());

        // dp[i,b]: mejor utilidad con los primeros i útiles y presupuesto b.
        var dp = new int[n + 1, presupuestoBs + 1];
        for (int i = 1; i <= n; i++)
        {
            Util actual = utiles[i - 1];
            for (int b = 0; b <= presupuestoBs; b++)
            {
                int noComprar = dp[i - 1, b];
                int comprar = actual.CostoBs <= b
                    ? dp[i - 1, b - actual.CostoBs] + actual.Utilidad
                    : int.MinValue;
                dp[i, b] = Math.Max(noComprar, comprar);
            }
        }

        int restante = presupuestoBs;
        var elegidos = new List<string>();
        for (int i = n; i >= 1; i--)
        {
            if (dp[i, restante] != dp[i - 1, restante])
            {
                Util x = utiles[i - 1];
                elegidos.Add(x.Codigo);
                restante -= x.CostoBs;
            }
        }
        elegidos.Reverse();
        _codigosElegidos = new HashSet<string>(elegidos, StringComparer.Ordinal);
        return (dp[n, presupuestoBs], elegidos);
    }
    public bool EstaEnLaCompra(string codigo) => _codigosElegidos.Contains(codigo);
}

public static class Programa
{
    private static Util[] Lote(params (string c, int costo, int u)[] d) =>
        d.Select(x => new Util(x.c, x.costo, x.u)).ToArray();

    private static readonly (string que, Util[] utiles, int presupuesto, int esperada, string[] codigos)[] Casos =
    {
        ("el voraz se equivoca", Lote(("U1",20,120),("U2",40,200),("U3",60,240)),100,440,new[]{"U2","U3"}),
        ("cuatro útiles", Lote(("U1",4,6),("U2",6,8),("U3",8,10),("U4",10,12)),16,20,new[]{"U2","U4"}),
        ("presupuesto cero", Lote(("U1",5,10),("U2",7,12)),0,0,Array.Empty<string>()),
        ("sin útiles", Lote(),30,0,Array.Empty<string>()),
        ("nada cabe", Lote(("U1",80,90),("U2",100,100)),60,0,Array.Empty<string>()),
        ("entran todos", Lote(("U1",10,20),("U2",14,24),("U3",6,8)),200,52,new[]{"U1","U2","U3"})
    };

    private static void Probar(string titulo, Func<IComprador> fabrica)
    {
        int bien=0,total=0;
        foreach (var (que, utiles, presupuesto, esperada, codigos) in Casos)
        {
            var c=fabrica(); var (u,e)=c.Elegir(utiles,presupuesto); total++;
            if (u==esperada && e.OrderBy(x=>x).SequenceEqual(codigos.OrderBy(x=>x))) bien++;
            else Console.WriteLine($"MAL en {que}: esperado {esperada} [{string.Join(",",codigos)}], obtuvo {u} [{string.Join(",",e)}]");
        }
        var consulta=fabrica(); consulta.Elegir(Casos[0].utiles,Casos[0].presupuesto); total+=2;
        if(consulta.EstaEnLaCompra("U2")) bien++; else Console.WriteLine("MAL EstaEnLaCompra U2");
        if(!consulta.EstaEnLaCompra("U1")) bien++; else Console.WriteLine("MAL EstaEnLaCompra U1");
        Console.WriteLine($"{titulo}: {bien} de {total} pruebas correctas");
    }

    private static (Util[] utiles,int presupuesto) CrearCarga(int n)
    {
        var r=new Random(n+1000);
        var a=Enumerable.Range(0,n).Select(i=>new Util("K"+i,r.Next(4,50),r.Next(10,100))).ToArray();
        return (a,a.Sum(x=>x.CostoBs)/3);
    }

    private static double Cronometrar(Func<int> f)
    {
        var sw=Stopwatch.StartNew(); _=f(); sw.Stop(); return sw.Elapsed.TotalMilliseconds;
    }

    private static void MedirTamanio(int n,List<string> filas)
    {
        var (a,b)=CrearCarga(n);
        var ing=new CompradorIngenuo(); var mej=new CompradorMejorado();
        int ui=ing.Elegir(a,b).utilidad, um=mej.Elegir(a,b).utilidad;
        if(ui!=um) throw new Exception($"Utilidades diferentes para n={n}: {ui} y {um}");
        _=ing.Elegir(a,b); _=mej.Elegir(a,b); // calentamiento
        for(int r=1;r<=3;r++)
        {
            double ti=Cronometrar(()=>ing.Elegir(a,b).utilidad);
            double tm=Cronometrar(()=>mej.Elegir(a,b).utilidad);
            filas.Add(string.Join(",",n,r,ti.ToString("F4",CultureInfo.InvariantCulture),
                tm.ToString("F4",CultureInfo.InvariantCulture),ui,um,"true"));
        }
        Console.WriteLine($"n={n}, B={b}, utilidad={ui}: resultados coinciden.");
    }

    private static void MedirMil()
    {
        const int n=1000,b=10000;
        var r=new Random(n+1000);
        var a=Enumerable.Range(0,n).Select(i=>new Util("K"+i,r.Next(4,50),r.Next(10,100))).ToArray();
        var c=new CompradorMejorado();
        _=c.Elegir(a,b); // calentamiento
        var tiempos=new List<double>(); int utilidad=0;
        for(int i=1;i<=3;i++)
        {
            var sw=Stopwatch.StartNew(); utilidad=c.Elegir(a,b).utilidad; sw.Stop();
            tiempos.Add(sw.Elapsed.TotalMilliseconds);
        }
        var lineas=new List<string>{"n,presupuesto,corrida,tiempo_ms,utilidad"};
        for(int i=0;i<3;i++) lineas.Add($"1000,10000,{i+1},{tiempos[i].ToString("F4",CultureInfo.InvariantCulture)},{utilidad}");
        double promedio=tiempos.Average();
        lineas.Add($"1000,10000,promedio,{promedio.ToString("F4",CultureInfo.InvariantCulture)},{utilidad}");
        File.WriteAllLines("integradora_1000.csv",lineas,new UTF8Encoding(true));
        Console.WriteLine($"Mejorada n=1000, B=10000: promedio {promedio:F4} ms; utilidad {utilidad}.");
    }

    public static void Main(string[] args)
    {
        int inicial=18;
        if(args.Length>0 && (!int.TryParse(args[0],out inicial)||inicial<1||inicial+6>=63))
        { Console.WriteLine("Uso: dotnet run -c Release -- [nInicial], nInicial >= 1 y nInicial+6 < 63."); return; }
        Probar("CompradorIngenuo",()=>new CompradorIngenuo());
        Probar("CompradorMejorado",()=>new CompradorMejorado());

        int[] ns={inicial,inicial+2,inicial+4,inicial+6};
        var detalle=new List<string>{"n,corrida,ingenua_ms,mejorada_ms,utilidad_ingenua,utilidad_mejorada,utilidades_iguales"};
        foreach(int n in ns) MedirTamanio(n,detalle);
        File.WriteAllLines("integradora_detalle.csv",detalle,new UTF8Encoding(true));

        var resumen=new List<string>{"n,ingenua_promedio_ms,factor_ingenua,mejorada_promedio_ms,factor_mejorada,utilidad,utilidades_iguales"};
        double antI=0,antM=0;
        foreach(int n in ns)
        {
            var f=detalle.Skip(1).Select(x=>x.Split(',')).Where(x=>int.Parse(x[0])==n).ToArray();
            double pi=f.Average(x=>double.Parse(x[2],CultureInfo.InvariantCulture));
            double pm=f.Average(x=>double.Parse(x[3],CultureInfo.InvariantCulture));
            string fi=antI==0?"":(pi/antI).ToString("F3",CultureInfo.InvariantCulture);
            string fm=antM==0?"":(pm/antM).ToString("F3",CultureInfo.InvariantCulture);
            bool iguales=f.All(x=>x[4]==x[5]&&x[6]=="true");
            resumen.Add(string.Join(",",n,pi.ToString("F4",CultureInfo.InvariantCulture),fi,
                pm.ToString("F4",CultureInfo.InvariantCulture),fm,f[0][4],iguales.ToString().ToLowerInvariant()));
            antI=pi; antM=pm;
        }
        File.WriteAllLines("integradora_resumen.csv",resumen,new UTF8Encoding(true));
        Console.WriteLine("Generados integradora_detalle.csv e integradora_resumen.csv.");
        MedirMil();
    }
}

