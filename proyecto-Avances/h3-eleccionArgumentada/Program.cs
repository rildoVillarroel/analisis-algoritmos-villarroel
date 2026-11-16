Console.OutputEncoding = System.Text.Encoding.UTF8;

var caso = args.Length > 0 ? args[0].ToLowerInvariant() : "eleccion";

switch (caso)
{
    case "datos":    C6.Eleccion.Duelo.Datos(); break;
    case "duelo":    C6.Eleccion.Duelo.DuelosPequenos(); break;
    case "escala":   C6.Eleccion.Duelo.Escala(); break;
    case "calidad":  C6.Eleccion.Duelo.Calidad(); break;
    case "eleccion": C6.Eleccion.Duelo.Correr(); break;
    default:
        Console.WriteLine("Uso: dotnet run -c Release -- [datos | duelo | escala | calidad | eleccion]");
        break;
}