using SistemaLavanderia.Core.Enums;

namespace SistemaLavanderia.Web.Services;

public static class CalculoPrecoService
{
    // Tabela de preços base por tipo de roupa e serviço
    private static readonly Dictionary<(TipoServico, TipoRoupa), decimal> TabelaPrecos = new()
    {
        // Lavagem Simples
        { (TipoServico.LavagemSimples, TipoRoupa.Camisa), 8.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Calca), 10.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Vestido), 15.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Blusa), 8.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Bermuda), 7.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Jaqueta), 18.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Saia), 9.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Toalha), 12.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Lenco), 5.00m },
        { (TipoServico.LavagemSimples, TipoRoupa.Outros), 10.00m },

        // Lavagem a Seco
        { (TipoServico.LavagemSeco, TipoRoupa.Camisa), 15.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Calca), 18.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Vestido), 25.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Blusa), 15.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Jaqueta), 30.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Saia), 16.00m },
        { (TipoServico.LavagemSeco, TipoRoupa.Outros), 20.00m },

        // Passadoria
        { (TipoServico.Passadoria, TipoRoupa.Camisa), 5.00m },
        { (TipoServico.Passadoria, TipoRoupa.Calca), 6.00m },
        { (TipoServico.Passadoria, TipoRoupa.Vestido), 8.00m },
        { (TipoServico.Passadoria, TipoRoupa.Blusa), 5.00m },
        { (TipoServico.Passadoria, TipoRoupa.Bermuda), 4.00m },
        { (TipoServico.Passadoria, TipoRoupa.Saia), 5.00m },

        // Lavagem Pesada (roupas muito sujas)
        { (TipoServico.LavagemPesada, TipoRoupa.Calca), 20.00m },
        { (TipoServico.LavagemPesada, TipoRoupa.Jaqueta), 35.00m },
        { (TipoServico.LavagemPesada, TipoRoupa.Toalha), 25.00m },
        { (TipoServico.LavagemPesada, TipoRoupa.Outros), 25.00m },

        // Tinturaria
        { (TipoServico.Tinturaria, TipoRoupa.Calca), 40.00m },
        { (TipoServico.Tinturaria, TipoRoupa.Vestido), 60.00m },
        { (TipoServico.Tinturaria, TipoRoupa.Jaqueta), 70.00m }
    };

    public static decimal CalcularPreco(TipoServico servico, TipoRoupa roupa)
    {
        return TabelaPrecos.GetValueOrDefault((servico, roupa), 15.00m); // Valor padrão
    }

    public static List<TipoRoupa> ObterRoupasPorServico(TipoServico servico)
    {
        return TabelaPrecos.Keys
            .Where(k => k.Item1 == servico)
            .Select(k => k.Item2)
            .Distinct()
            .ToList();
    }

    public static string ObterDescricaoServico(TipoServico servico)
    {
        return servico switch
        {
            TipoServico.LavagemSimples => "Lavagem Simples (água e sabão)",
            TipoServico.LavagemSeco => "Lavagem a Seco (produtos especiais)",
            TipoServico.Passadoria => "Passadoria (roupas passadas)",
            TipoServico.LavagemPesada => "Lavagem Pesada (roupas muito sujas)",
            TipoServico.Tinturaria => "Tinturaria (tingimento e restauração)",
            _ => "Outros serviços"
        };
    }
}