using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterfaceTeste
{
    public sealed class LoginForm : Form
    {
        private readonly TextBox _txtServidor;
        private readonly TextBox _txtBanco;
        private readonly TextBox _txtUsuario;
        private readonly TextBox _txtSenha;
        private readonly CheckBox _chkWindows;
        private readonly CheckBox _chkMostrarSenha;
        private readonly Button _btnConectar;
        private readonly Label _lblStatus;

        public LoginForm()
        {
            Text = "Acesso ao Banco de Dados";
            Icon = AppIcon.Carregar();
            ClientSize = new Size(540, 470);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Color.FromArgb(230, 81, 0)
            };

            var titulo = new Label
            {
                Text = "Acesso ao SQL Server",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 18)
            };

            var subtitulo = new Label
            {
                Text = "Informe as credenciais para conectar ao banco de dados",
                ForeColor = Color.FromArgb(255, 224, 178),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(30, 58)
            };

            header.Controls.Add(titulo);
            header.Controls.Add(subtitulo);

            var lblServidor = CriarLabel("Servidor:", 130);
            _txtServidor = CriarTextBox(126);
            _txtServidor.Text = ".";

            var lblBanco = CriarLabel("Banco de dados:", 180);
            _txtBanco = CriarTextBox(176);

            var lblUsuario = CriarLabel("Usuário:", 230);
            _txtUsuario = CriarTextBox(226);

            var lblSenha = CriarLabel("Senha:", 280);
            _txtSenha = CriarTextBox(276);
            _txtSenha.UseSystemPasswordChar = true;

            _chkWindows = new CheckBox
            {
                Text = "Usar autenticação do Windows",
                Location = new Point(180, 316),
                AutoSize = true,
                Checked = false
            };
            _chkWindows.CheckedChanged += (sender, args) =>
            {
                bool windows = _chkWindows.Checked;
                _txtUsuario.Enabled = !windows;
                _txtSenha.Enabled = !windows;
            };

            _chkMostrarSenha = new CheckBox
            {
                Text = "Mostrar senha",
                Location = new Point(390, 276),
                AutoSize = true
            };
            _chkMostrarSenha.CheckedChanged += (sender, args) =>
            {
                _txtSenha.UseSystemPasswordChar = !_chkMostrarSenha.Checked;
            };

            _lblStatus = new Label
            {
                Location = new Point(32, 348),
                Size = new Size(476, 20),
                ForeColor = Color.FromArgb(97, 97, 97),
                Text = "Aguardando conexão..."
            };

            _btnConectar = new Button
            {
                Text = "Conectar",
                Location = new Point(180, 380),
                Size = new Size(150, 42),
                BackColor = Color.FromArgb(230, 81, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnConectar.FlatAppearance.BorderSize = 0;
            _btnConectar.Click += async (sender, args) => await ConectarAsync();

            var btnSair = new Button
            {
                Text = "Sair",
                Location = new Point(346, 380),
                Size = new Size(150, 42),
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.FromArgb(33, 33, 33),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnSair.FlatAppearance.BorderSize = 0;
            btnSair.Click += (sender, args) => Close();

            Controls.Add(header);
            Controls.Add(lblServidor);
            Controls.Add(_txtServidor);
            Controls.Add(lblBanco);
            Controls.Add(_txtBanco);
            Controls.Add(lblUsuario);
            Controls.Add(_txtUsuario);
            Controls.Add(lblSenha);
            Controls.Add(_txtSenha);
            Controls.Add(_chkWindows);
            Controls.Add(_chkMostrarSenha);
            Controls.Add(_lblStatus);
            Controls.Add(_btnConectar);
            Controls.Add(btnSair);

            AcceptButton = _btnConectar;
        }

        private static Label CriarLabel(string texto, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(32, y + 6),
                Size = new Size(140, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(55, 55, 55)
            };
        }

        private static TextBox CriarTextBox(int y)
        {
            return new TextBox
            {
                Location = new Point(180, y),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10F)
            };
        }

        private string MontarConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _txtServidor.Text.Trim(),
                InitialCatalog = _txtBanco.Text.Trim(),
                IntegratedSecurity = _chkWindows.Checked,
                TrustServerCertificate = true,
                MultipleActiveResultSets = true,
                ConnectTimeout = 10,
                ApplicationName = "InterfaceTeste"
            };

            if (!_chkWindows.Checked)
            {
                builder.UserID = _txtUsuario.Text.Trim();
                builder.Password = _txtSenha.Text;
            }

            return builder.ConnectionString;
        }

        private async Task ConectarAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtServidor.Text))
            {
                MessageBox.Show(this, "Informe o servidor.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtServidor.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtBanco.Text))
            {
                MessageBox.Show(this, "Informe o banco de dados.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtBanco.Focus();
                return;
            }

            if (!_chkWindows.Checked && string.IsNullOrWhiteSpace(_txtUsuario.Text))
            {
                MessageBox.Show(this, "Informe o usuário.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsuario.Focus();
                return;
            }

            string connectionString = MontarConnectionString();

            _btnConectar.Enabled = false;
            _lblStatus.Text = "Conectando ao servidor...";

            BancoSqlServer banco = null;
            string versao = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    banco = BancoSqlServer.Abrir(connectionString);

                    using (var dr = banco.Consultar("SELECT @@VERSION"))
                    {
                        if (dr.Read())
                            versao = Convert.ToString(dr[0]) ?? string.Empty;
                    }
                });
            }
            catch (Exception ex)
            {
                if (banco != null)
                {
                    banco.Dispose();
                    banco = null;
                }

                _lblStatus.Text = "Falha na conexão.";
                MessageBox.Show(
                    this,
                    "Não foi possível acessar o banco de dados." + Environment.NewLine + Environment.NewLine + "Detalhes: " + ex.Message,
                    "Falha na conexão",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            finally
            {
                _btnConectar.Enabled = true;
            }

            string[] linhas = versao.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string resumo = linhas.Length > 0 ? linhas[0] : versao;

            _lblStatus.Text = "Conectado com sucesso.";
            MessageBox.Show(
                this,
                "Banco de dados acessado com sucesso!" + Environment.NewLine + Environment.NewLine + resumo,
                "Conexão bem-sucedida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Hide();

            try
            {
                using (var main = new MainForm(banco, _txtServidor.Text.Trim(), _txtBanco.Text.Trim()))
                {
                    main.ShowDialog();
                }

                Close();
            }
            finally
            {
                banco.Dispose();
            }
        }
    }
}
