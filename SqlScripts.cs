using System.Collections.Generic;

namespace InterfaceTeste
{
    internal static class SqlScripts
    {
        public sealed class ScriptItem
        {
            public ScriptItem(string titulo, string arquivo)
            {
                Titulo = titulo;
                Arquivo = arquivo;
            }

            public string Titulo { get; }

            public string Arquivo { get; }
        }

        public static readonly IReadOnlyList<ScriptItem> Todos = new List<ScriptItem>
        {
            new ScriptItem("Cliente", "01_Cliente.sql"),
            new ScriptItem("Fornecedor", "02_Fornecedor.sql"),
            new ScriptItem("Produto - Arroz", "03_Produto_Arroz.sql"),
            new ScriptItem("Produto - Abridor de latas", "04_Produto_Abridor.sql"),
            new ScriptItem("Produto - Carne", "05_Produto_Carne.sql"),
            new ScriptItem("Produto - Combustível", "06_Produto_Combustivel.sql"),
            new ScriptItem("PDV", "07_PDV.sql")
        };
    }
}
