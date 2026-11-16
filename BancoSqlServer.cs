using System.Data.SqlClient;
using Telecon.Genericos.Classes.BancoDeDados;

namespace InterfaceTeste
{
    /// <summary>
    /// Adaptador do <see cref="SqlNativo"/> que aceita qualquer connection string
    /// (autenticação SQL ou autenticação do Windows).
    /// </summary>
    public sealed class BancoSqlServer : SqlNativo
    {
        private BancoSqlServer()
            : base(string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public static BancoSqlServer Abrir(string connectionString)
        {
            var banco = new BancoSqlServer();
            banco.AbrirConexaoPersonalizada(connectionString);
            return banco;
        }

        private void AbrirConexaoPersonalizada(string connectionString)
        {
            var conexao = new SqlConnection(connectionString);
            conexao.Open();

            Conexao = conexao;
            Comandos = conexao.CreateCommand();
        }

        public void IniciarTransacao()
        {
            Comandos.Transaction = Conexao.BeginTransaction();
        }

        public void ConfirmarTransacao()
        {
            Comandos.Transaction.Commit();
            Comandos.Transaction = null;
        }

        public void CancelarTransacao()
        {
            try
            {
                if (Comandos != null && Comandos.Transaction != null)
                    Comandos.Transaction.Rollback();
            }
            catch
            {
                // Ignora falhas no rollback: o erro original é mais relevante.
            }
            finally
            {
                if (Comandos != null)
                    Comandos.Transaction = null;
            }
        }

        public new void Dispose()
        {
            if (Comandos != null)
            {
                Comandos.Dispose();
                Comandos = null;
            }

            if (Conexao != null)
            {
                Conexao.Dispose();
                Conexao = null;
            }
        }
    }
}
