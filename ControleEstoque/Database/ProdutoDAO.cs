using MySql.Data.MySqlClient;
using System.Collections.Generic;
using ControleEstoque.Models;

namespace ControleEstoque.Database
{
    public class ProdutoDAO
    {
        public List<Produto> ListarProdutos()
        {
            List<Produto> lista = new List<Produto>();

            Conexao conexao = new Conexao();

            using (MySqlConnection conn = conexao.GetConnection())
            {
                string sql = "SELECT nome, quantidade, preco FROM Produtos";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Produto p = new Produto();

                    p.Nome = reader.GetString("nome");
                    p.Quantidade = reader.GetInt32("quantidade");
                    p.Preco = reader.GetDecimal("preco");

                    lista.Add(p);
                }
            }

            return lista;
        }
    }
}