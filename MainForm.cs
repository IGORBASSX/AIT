using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterfaceTeste
{
    public sealed class MainForm : Form
    {
        private readonly BancoSqlServer _banco;
        private readonly TextBox _log;
        private readonly Label _lblStatus;
        private readonly List<Button> _botoes = new List<Button>();
        private bool _executando;

        public MainForm(BancoSqlServer banco, string servidor, string nomeBanco)
        {
            _banco = banco;

            Text = "Operações no Banco de Dados";
            Icon = AppIcon.Carregar();
            ClientSize = new Size(840, 650);
            MinimumSize = new Size(780, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 208F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(230, 81, 0)
            };

            var titulo = new Label
            {
                Text = "Testes via biblioteca",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 16)
            };

            var subtitulo = new Label
            {
                Text = "Servidor: " + servidor + "   |   Banco: " + nomeBanco,
                ForeColor = Color.FromArgb(255, 224, 178),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(26, 56)
            };

            header.Controls.Add(titulo);
            header.Controls.Add(subtitulo);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 14, 18, 8),
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.White
            };

            foreach (var tarefa in Tarefas.Todos)
            {
                flow.Controls.Add(CriarBotao(tarefa));
            }

            var btnTodos = new Button
            {
                Text = "Executar todos",
                Size = new Size(250, 48),
                Margin = new Padding(6),
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnTodos.FlatAppearance.BorderSize = 0;
            btnTodos.Click += async (sender, args) => await ExecutarTodosAsync();
            _botoes.Add(btnTodos);
            flow.Controls.Add(btnTodos);

            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 0, 18, 10),
                BackColor = Color.White
            };

            var logGroup = new GroupBox
            {
                Text = "Log de execução",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ForeColor = Color.FromArgb(55, 55, 55)
            };

            _log = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(250, 250, 250),
                Font = new Font("Consolas", 9.5F),
                BorderStyle = BorderStyle.None
            };

            logGroup.Controls.Add(_log);
            logPanel.Controls.Add(logGroup);

            var statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(242, 242, 242)
            };

            _lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Pronto para executar.",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            statusPanel.Controls.Add(_lblStatus);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(flow, 0, 1);
            root.Controls.Add(logPanel, 0, 2);
            root.Controls.Add(statusPanel, 0, 3);

            Controls.Add(root);
        }

        private Button CriarBotao(Tarefa tarefa)
        {
            var botao = new Button
            {
                Text = tarefa.Titulo,
                Size = new Size(250, 48),
                Margin = new Padding(6),
                BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.FromArgb(33, 33, 33),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            botao.Click += async (sender, args) => await ExecutarUmaAsync(tarefa);
            _botoes.Add(botao);
            return botao;
        }

        private async Task ExecutarUmaAsync(Tarefa tarefa)
        {
            if (_executando)
            {
                return;
            }

            DefinirOcupado(true);
            try
            {
                string erro = await ExecutarTarefaAsync(tarefa);

                if (erro == null)
                {
                    _lblStatus.Text = "Concluído com sucesso: " + tarefa.Titulo;
                    MessageBox.Show(
                        this,
                        "A operação \"" + tarefa.Titulo + "\" foi executada com sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    _lblStatus.Text = "Falha ao executar: " + tarefa.Titulo;
                    MessageBox.Show(
                        this,
                        "Não foi possível executar a operação \"" + tarefa.Titulo + "\"." + Environment.NewLine + Environment.NewLine + "Detalhes: " + erro,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                DefinirOcupado(false);
            }
        }

        private async Task ExecutarTodosAsync()
        {
            if (_executando)
            {
                return;
            }

            DefinirOcupado(true);
            try
            {
                int sucessos = 0;
                var falhas = new List<string>();

                foreach (var tarefa in Tarefas.Todos)
                {
                    string erro = await ExecutarTarefaAsync(tarefa);

                    if (erro == null)
                    {
                        sucessos++;
                    }
                    else
                    {
                        falhas.Add(tarefa.Titulo + ": " + erro);
                    }
                }

                if (falhas.Count == 0)
                {
                    _lblStatus.Text = "Todas as operações foram executadas com sucesso.";
                    MessageBox.Show(
                        this,
                        "Todas as " + sucessos + " operações foram executadas com sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    _lblStatus.Text = "Execução concluída com falhas.";
                    MessageBox.Show(
                        this,
                        "Execução concluída." + Environment.NewLine + Environment.NewLine +
                        "Sucessos: " + sucessos + Environment.NewLine +
                        "Falhas: " + falhas.Count + Environment.NewLine + Environment.NewLine +
                        string.Join(Environment.NewLine, falhas),
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                DefinirOcupado(false);
            }
        }

        private async Task<string> ExecutarTarefaAsync(Tarefa tarefa)
        {
            Registrar("Iniciando: " + tarefa.Titulo + " (" + tarefa.Descricao + ")");
            _lblStatus.Text = "Executando: " + tarefa.Titulo + "...";

            string erro = await Task.Run(() => ExecutarNoBanco(tarefa));

            if (erro == null)
                Registrar("Sucesso: " + tarefa.Titulo);
            else
                Registrar("ERRO em " + tarefa.Titulo + ": " + erro);

            return erro;
        }

        private string ExecutarNoBanco(Tarefa tarefa)
        {
            try
            {
                _banco.IniciarTransacao();

                try
                {
                    tarefa.Executar(_banco);
                    _banco.ConfirmarTransacao();
                    return null;
                }
                catch
                {
                    _banco.CancelarTransacao();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void Registrar(string mensagem)
        {
            _log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + mensagem + Environment.NewLine);
        }

        private void DefinirOcupado(bool ocupado)
        {
            _executando = ocupado;

            foreach (var botao in _botoes)
            {
                botao.Enabled = !ocupado;
            }

            Cursor = ocupado ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
