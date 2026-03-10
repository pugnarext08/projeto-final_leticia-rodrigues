using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ControleEstoque
{
    public partial class Form1 : Form
    {
        // TextBoxes
        TextBox txtNome, txtCodigo, txtQuantidade, txtPreco, txtBusca;

        // Buttons
        Button btnAdicionar, btnAtualizar, btnRemover, btnLimpar;

        // ListView e label de total
        ListView listaProdutos;
        Label lblTotal;

        // Para arredondar botões
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public Form1()
        {
            InicializarInterface();
            CarregarProdutos();
        }

        private void InicializarInterface()
        {
            // Configurações do Form
            this.Text = "Controle de Estoque";
            this.Width = 950;
            this.Height = 650;
            this.BackColor = Color.LightSkyBlue;
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            Font fonte = new Font("Segoe UI", 10);

            // Painel de cadastro
            Panel painelForm = new Panel
            {
                Width = 420,
                Height = 550,
                Left = 20,
                Top = 20,
                BackColor = Color.AliceBlue
            };

            Label titulo = new Label
            {
                Text = "Cadastro de Produto",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Top = 20,
                Left = 40,
                AutoSize = true,
                ForeColor = Color.White
            };

            // Campos do formulário
            Label lblNome = new Label { Text = "Nome", Top = 80, Left = 20, ForeColor = Color.White };
            txtNome = new TextBox { Top = 100, Left = 20, Width = 370, PlaceholderText = "Digite o nome do produto" };

            Label lblCodigo = new Label { Text = "Código", Top = 140, Left = 20, ForeColor = Color.White };
            txtCodigo = new TextBox { Top = 160, Left = 20, Width = 370, PlaceholderText = "Digite o código do produto" };

            Label lblQuantidade = new Label { Text = "Quantidade", Top = 200, Left = 20, ForeColor = Color.White };
            txtQuantidade = new TextBox { Top = 220, Left = 20, Width = 370, PlaceholderText = "Digite a quantidade" };

            Label lblPreco = new Label { Text = "Preço", Top = 260, Left = 20, ForeColor = Color.White };
            txtPreco = new TextBox { Top = 280, Left = 20, Width = 370, PlaceholderText = "Digite o preço" };

            // Botões com cores e arredondados
            btnAdicionar = CriarBotao("Adicionar", 20, 330, Color.MediumSeaGreen);
            btnAdicionar.Click += AdicionarProduto;

            btnAtualizar = CriarBotao("Atualizar", 140, 330, Color.DodgerBlue);
            btnAtualizar.Click += AtualizarProduto;

            btnRemover = CriarBotao("Remover", 260, 330, Color.IndianRed);
            btnRemover.Click += RemoverProduto;

            btnLimpar = CriarBotao("Limpar", 140, 380, Color.Gray);
            btnLimpar.Click += LimparCampos;

            // Tooltip
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtNome, "Informe o nome completo do produto");
            toolTip.SetToolTip(txtCodigo, "Informe o código do produto");
            toolTip.SetToolTip(txtQuantidade, "Informe a quantidade em estoque");
            toolTip.SetToolTip(txtPreco, "Informe o preço unitário");

            // Busca
            Label lblBusca = new Label { Text = "Buscar:", Top = 20, Left = 460, ForeColor = Color.White };
            txtBusca = new TextBox { Top = 40, Left = 520, Width = 350, PlaceholderText = "Digite nome ou código" };
            txtBusca.TextChanged += BuscarProduto;

            // Lista de produtos
            listaProdutos = new ListView
            {
                Left = 460,
                Top = 70,
                Width = 410,
                Height = 450,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.AliceBlue,
                ForeColor = Color.Black
            };

            listaProdutos.Columns.Add("ID", 40);
            listaProdutos.Columns.Add("Nome", 120);
            listaProdutos.Columns.Add("Código", 80);
            listaProdutos.Columns.Add("Qtd", 60);
            listaProdutos.Columns.Add("Preço", 80);
            listaProdutos.SelectedIndexChanged += SelecionarProduto;

            // Label de total
            lblTotal = new Label
            {
                Left = 460,
                Top = 530,
                Width = 400,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            // Adiciona controles ao painel
            painelForm.Controls.Add(titulo);
            painelForm.Controls.Add(lblNome);
            painelForm.Controls.Add(txtNome);
            painelForm.Controls.Add(lblCodigo);
            painelForm.Controls.Add(txtCodigo);
            painelForm.Controls.Add(lblQuantidade);
            painelForm.Controls.Add(txtQuantidade);
            painelForm.Controls.Add(lblPreco);
            painelForm.Controls.Add(txtPreco);
            painelForm.Controls.Add(btnAdicionar);
            painelForm.Controls.Add(btnAtualizar);
            painelForm.Controls.Add(btnRemover);
            painelForm.Controls.Add(btnLimpar);

            // Adiciona ao Form
            this.Controls.Add(painelForm);
            this.Controls.Add(lblBusca);
            this.Controls.Add(txtBusca);
            this.Controls.Add(listaProdutos);
            this.Controls.Add(lblTotal);
        }

        private Button CriarBotao(string texto, int left, int top, Color cor)
        {
            Button btn = new Button
            {
                Text = texto,
                Left = left,
                Top = top,
                Width = 110,
                Height = 35,
                BackColor = cor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10));
            return btn;
        }

        private MySqlConnection GetConnection()
        {
            string connStr = "server=localhost;user=root;password=;database=ControleEstoque;";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            return conn;
        }

        private void CarregarProdutos()
        {
            try
            {
                listaProdutos.Items.Clear();
                decimal total = 0;

                using (MySqlConnection conn = GetConnection())
                {
                    string sql = "SELECT id,nome,codigo,quantidade,preco FROM Produtos";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["id"].ToString());
                            item.SubItems.Add(reader["nome"].ToString());
                            item.SubItems.Add(reader["codigo"].ToString());
                            item.SubItems.Add(reader["quantidade"].ToString());
                            item.SubItems.Add(Convert.ToDecimal(reader["preco"]).ToString("C"));
                            listaProdutos.Items.Add(item);

                            total += Convert.ToDecimal(reader["preco"]) * Convert.ToInt32(reader["quantidade"]);
                        }
                    }
                }

                lblTotal.Text = "Valor Total em Estoque: " + total.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message);
            }
        }

        private void AdicionarProduto(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    string sql = "INSERT INTO Produtos(nome,codigo,quantidade,preco) VALUES (@nome,@codigo,@quantidade,@preco)";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@codigo", txtCodigo.Text);
                    cmd.Parameters.AddWithValue("@quantidade", int.Parse(txtQuantidade.Text));
                    cmd.Parameters.AddWithValue("@preco", decimal.Parse(txtPreco.Text));
                    cmd.ExecuteNonQuery();
                }

                CarregarProdutos();
                LimparCampos(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao adicionar: " + ex.Message);
            }
        }

        private void AtualizarProduto(object sender, EventArgs e)
        {
            if (listaProdutos.SelectedItems.Count == 0) return;
            if (!ValidarCampos()) return;

            try
            {
                int id = int.Parse(listaProdutos.SelectedItems[0].Text);

                using (MySqlConnection conn = GetConnection())
                {
                    string sql = "UPDATE Produtos SET nome=@nome,codigo=@codigo,quantidade=@quantidade,preco=@preco WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@codigo", txtCodigo.Text);
                    cmd.Parameters.AddWithValue("@quantidade", int.Parse(txtQuantidade.Text));
                    cmd.Parameters.AddWithValue("@preco", decimal.Parse(txtPreco.Text));
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                CarregarProdutos();
                LimparCampos(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar: " + ex.Message);
            }
        }

        private void RemoverProduto(object sender, EventArgs e)
        {
            if (listaProdutos.SelectedItems.Count == 0) return;

            try
            {
                int id = int.Parse(listaProdutos.SelectedItems[0].Text);

                using (MySqlConnection conn = GetConnection())
                {
                    string sql = "DELETE FROM Produtos WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                CarregarProdutos();
                LimparCampos(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao remover: " + ex.Message);
            }
        }

        private void BuscarProduto(object sender, EventArgs e)
        {
            string busca = txtBusca.Text.ToLower();

            foreach (ListViewItem item in listaProdutos.Items)
            {
                item.BackColor = Color.AliceBlue;
                if (!item.SubItems[1].Text.ToLower().Contains(busca) &&
                    !item.SubItems[2].Text.ToLower().Contains(busca))
                {
                    item.BackColor = Color.LightGray;
                }
            }
        }

        private void SelecionarProduto(object sender, EventArgs e)
        {
            if (listaProdutos.SelectedItems.Count == 0) return;

            var item = listaProdutos.SelectedItems[0];

            txtNome.Text = item.SubItems[1].Text;
            txtCodigo.Text = item.SubItems[2].Text;
            txtQuantidade.Text = item.SubItems[3].Text;
            txtPreco.Text = decimal.Parse(item.SubItems[4].Text, System.Globalization.NumberStyles.Currency).ToString();
        }

        private void LimparCampos(object sender, EventArgs e)
        {
            txtNome.Text = "";
            txtCodigo.Text = "";
            txtQuantidade.Text = "";
            txtPreco.Text = "";
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtQuantidade.Text) ||
                string.IsNullOrWhiteSpace(txtPreco.Text))
            {
                MessageBox.Show("Preencha todos os campos!");
                return false;
            }

            if (!int.TryParse(txtQuantidade.Text, out _) ||
                !decimal.TryParse(txtPreco.Text, out _))
            {
                MessageBox.Show("Quantidade ou Preço inválidos!");
                return false;
            }

            return true;
        }
    }
}