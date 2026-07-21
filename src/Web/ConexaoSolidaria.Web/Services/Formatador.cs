using System.Globalization;

namespace ConexaoSolidaria.Web.Services;

public static class Formatador
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C2", PtBr);

    public static string Data(DateTime data) => data.ToLocalTime().ToString("dd/MM/yyyy", PtBr);

    public static string DataHora(DateTime data) => data.ToLocalTime().ToString("dd/MM/yyyy HH:mm", PtBr);
}
