using System;
using System.Collections.Generic;
using Telecon.Genericos.Classes.BancoDeDados;

namespace InterfaceTeste
{
    /// <summary>
    /// Uma operação disponível na tela principal.
    /// </summary>
    public sealed class Tarefa
    {
        public Tarefa(string titulo, string descricao, Action<IBanco> executar)
        {
            Titulo = titulo;
            Descricao = descricao;
            Executar = executar;
        }

        public string Titulo { get; }

        public string Descricao { get; }

        public Action<IBanco> Executar { get; }
    }

    /// <summary>
    /// Lista de operações que podem ser executadas na tela principal.
    /// </summary>
    public static class Tarefas
    {
        public static readonly IList<Tarefa> Todos = new List<Tarefa>
        {
            new Tarefa("Inserir cliente", "Tabela Clientes", OperacoesTelecode.InserirCliente),
            new Tarefa("Inserir fornecedor", "Tabela Fornecedores", OperacoesTelecode.InserirFornecedor),
            new Tarefa("Inserir produto - Arroz", "Tabela Produtos", OperacoesTelecode.InserirProdutoArroz),
            new Tarefa("Inserir produto - Abridor", "Tabela Produtos", OperacoesTelecode.InserirProdutoAbridor),
            new Tarefa("Inserir produto - Carne", "Tabela Produtos", OperacoesTelecode.InserirProdutoCarne),
            new Tarefa("Inserir produto - Combustível", "Tabela Produtos", OperacoesTelecode.InserirProdutoCombustivel),
            new Tarefa("Inserir PDV", "Tabela PDVs", OperacoesTelecode.InserirPdv)
        };
    }
}
