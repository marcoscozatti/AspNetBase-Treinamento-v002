// =====================================================================
// DAO — Data Access Object (Objeto de Acesso a Dados)
// ---------------------------------------------------------------------
// Esta é a classe MAIS IMPORTANTE da aula de hoje.
// Ela concentra TODOS os comandos SQL do sistema:
//
//     SELECT  -> Listar() e BuscarPorId()
//     INSERT  -> Inserir()
//     UPDATE  -> Atualizar()
//     DELETE  -> Excluir()
//
// Usamos o ADO.NET "puro", com as classes clássicas:
//
//   SqlConnection  -> a CONEXÃO com o SQL Server (abre / fecha).
//   SqlCommand     -> o COMANDO SQL a ser executado (o texto do SQL).
//   SqlParameter   -> os VALORES enviados ao comando (@Nome, @Idade...).
//   SqlDataReader  -> o "LEITOR" que percorre as linhas de um SELECT.
//
// Os três jeitos de executar um SqlCommand:
//   ExecuteReader()    -> quando o SQL DEVOLVE LINHAS (SELECT).
//   ExecuteNonQuery()  -> quando o SQL NÃO devolve linhas (INSERT/UPDATE/DELETE);
//                         retorna a QUANTIDADE de linhas afetadas.
//   ExecuteScalar()    -> quando o SQL devolve UM ÚNICO valor
//                         (ex.: COUNT(*), ou o Id recém-inserido).
//
// Sobre o pacote: usamos "Microsoft.Data.SqlClient", que é a versão atual
// do antigo "System.Data.SqlClient". As classes têm os MESMOS nomes
// (SqlConnection, SqlCommand...) e funcionam do mesmo jeito; só muda o
// "using" no topo do arquivo.
// =====================================================================

using Microsoft.Data.SqlClient;   // SqlConnection, SqlCommand, SqlDataReader, SqlParameter
using System.Data;                // SqlDbType (tipos das colunas do SQL Server)
using CrudPessoasAdoNet.Models;

namespace CrudPessoasAdoNet.Data
{
    public class PessoaDAO
    {
        // Guarda a string de conexão lida do appsettings.json.
        private readonly string _connectionString;

        // INJEÇÃO DE DEPENDÊNCIA:
        // O ASP.NET nos entrega o IConfiguration, que dá acesso ao appsettings.json.
        // Dali lemos a string de conexão "CrudConsoleDB".
        public PessoaDAO(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CrudConsoleDB")
                ?? throw new InvalidOperationException(
                       "String de conexão 'CrudConsoleDB' não encontrada no appsettings.json.");
        }

        // =============================================================
        // SELECT — Lista TODAS as pessoas
        // =============================================================
        public List<Pessoa> Listar()
        {
            var lista = new List<Pessoa>();

            // 1) O comando SQL, exatamente como escreveríamos no SQL Server Management Studio.
            string sql = "SELECT Id, Nome, Idade FROM Pessoas ORDER BY Nome";

            // 2) "using" garante que a conexão será FECHADA e liberada no final,
            //    mesmo que aconteça um erro no meio do caminho.
            //    (Esquecer de fechar conexões é um erro clássico!)
            using (SqlConnection conexao = new SqlConnection(_connectionString))
            // 3) Cria o comando, dizendo QUAL SQL executar e EM QUAL conexão.
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                // 4) Abre a conexão com o banco.
                conexao.Open();

                // 5) ExecuteReader: executa o SELECT e devolve um "leitor" de linhas.
                using (SqlDataReader leitor = comando.ExecuteReader())
                {
                    // 6) leitor.Read() avança para a PRÓXIMA linha.
                    //    Retorna true enquanto houver linhas e false quando acabar.
                    while (leitor.Read())
                    {
                        // 7) Para cada linha, criamos um objeto Pessoa e copiamos
                        //    o valor de cada coluna para a propriedade correspondente.
                        //    Isso é o que o Entity Framework fazia "escondido"!
                        Pessoa pessoa = new Pessoa
                        {
                            Id    = leitor.GetInt32(leitor.GetOrdinal("Id")),
                            Nome  = leitor.GetString(leitor.GetOrdinal("Nome")),
                            Idade = leitor.GetInt32(leitor.GetOrdinal("Idade"))
                        };

                        lista.Add(pessoa);
                    }
                }
            } // <- aqui o "using" fecha o leitor, o comando e a conexão.

            return lista;
        }

        // =============================================================
        // SELECT — Busca UMA pessoa pelo Id
        // =============================================================
        // Retorna "Pessoa?" (pode ser null) caso o Id não exista.
        public Pessoa? BuscarPorId(int id)
        {
            // @Id é um PARÂMETRO: um "espaço reservado" que será preenchido depois.
            string sql = "SELECT Id, Nome, Idade FROM Pessoas WHERE Id = @Id";

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                // Preenche o parâmetro @Id com o valor recebido.
                // NUNCA monte o SQL concatenando texto, como:
                //     "SELECT ... WHERE Id = " + id      <-- ERRADO!
                // Isso abre a porta para o ataque de SQL INJECTION.
                // Com parâmetros, o valor é enviado separado do comando
                // e o banco nunca o interpreta como código SQL.
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conexao.Open();

                using (SqlDataReader leitor = comando.ExecuteReader())
                {
                    // Aqui usamos "if" em vez de "while" porque esperamos
                    // no máximo UMA linha (Id é chave primária).
                    if (leitor.Read())
                    {
                        return new Pessoa
                        {
                            Id    = leitor.GetInt32(leitor.GetOrdinal("Id")),
                            Nome  = leitor.GetString(leitor.GetOrdinal("Nome")),
                            Idade = leitor.GetInt32(leitor.GetOrdinal("Idade"))
                        };
                    }
                }
            }

            // Nenhuma linha encontrada.
            return null;
        }

        // =============================================================
        // INSERT — Grava uma NOVA pessoa
        // =============================================================
        // Retorna o Id gerado pelo banco para o novo registro.
        public int Inserir(Pessoa pessoa)
        {
            // Não enviamos o Id: a coluna é IDENTITY, o banco gera sozinho.
            // "OUTPUT INSERTED.Id" faz o SQL Server DEVOLVER o Id recém-criado.
            string sql = @"INSERT INTO Pessoas (Nome, Idade)
                           OUTPUT INSERTED.Id
                           VALUES (@Nome, @Idade)";

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                // Um parâmetro para cada valor. Informamos o TIPO e o TAMANHO
                // iguais aos da coluna no banco (NVARCHAR(100) e INT).
                comando.Parameters.Add("@Nome",  SqlDbType.NVarChar, 100).Value = pessoa.Nome;
                comando.Parameters.Add("@Idade", SqlDbType.Int).Value           = pessoa.Idade;

                conexao.Open();

                // ExecuteScalar: executa e devolve o valor da 1ª coluna da 1ª linha
                // — no nosso caso, o Id vindo do "OUTPUT INSERTED.Id".
                // Ele retorna "object", por isso convertemos para int.
                int novoId = Convert.ToInt32(comando.ExecuteScalar());
                return novoId;
            }
        }

        // =============================================================
        // UPDATE — Altera uma pessoa existente
        // =============================================================
        // Retorna true se alguma linha foi alterada (ou seja, o Id existia).
        public bool Atualizar(Pessoa pessoa)
        {
            // O WHERE é ESSENCIAL: sem ele, o UPDATE alteraria TODAS as linhas!
            string sql = @"UPDATE Pessoas
                              SET Nome  = @Nome,
                                  Idade = @Idade
                            WHERE Id = @Id";

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                comando.Parameters.Add("@Nome",  SqlDbType.NVarChar, 100).Value = pessoa.Nome;
                comando.Parameters.Add("@Idade", SqlDbType.Int).Value           = pessoa.Idade;
                comando.Parameters.Add("@Id",    SqlDbType.Int).Value           = pessoa.Id;

                conexao.Open();

                // ExecuteNonQuery: usado quando o SQL NÃO devolve linhas.
                // Retorna QUANTAS linhas foram afetadas pelo comando.
                int linhasAfetadas = comando.ExecuteNonQuery();

                // Se afetou 0 linhas, é porque não existe pessoa com esse Id.
                return linhasAfetadas > 0;
            }
        }

        // =============================================================
        // DELETE — Exclui uma pessoa
        // =============================================================
        // Retorna true se alguma linha foi excluída.
        public bool Excluir(int id)
        {
            // Mesmo cuidado do UPDATE: sem WHERE, apagaria a tabela INTEIRA!
            string sql = "DELETE FROM Pessoas WHERE Id = @Id";

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conexao.Open();

                int linhasAfetadas = comando.ExecuteNonQuery();
                return linhasAfetadas > 0;
            }
        }
    }
}
