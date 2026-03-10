using MySql.Data.MySqlClient;

namespace ControleEstoque.Database
{
    public class Conexao
    {
        // Configure aqui com os dados que você usa no Workbench
        private string connectionString =
            "server=localhost;" +   // host do MySQL
            "port=3306;" +          // porta padrão
            "database=ControleEstoque;" +  // nome do banco
            "user=root;" +          // seu usuário MySQL
            "password=;" +          // sua senha
            "SslMode=Preferred;";   // opcional, evita problemas de SSL

        // Retorna a conexão aberta
        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();  // abre a conexão
            return conn;
        }
    }
}