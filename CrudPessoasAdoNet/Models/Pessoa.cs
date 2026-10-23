// =====================================================================
// MODEL (o "M" do MVC)
// ---------------------------------------------------------------------
// Esta classe "Pessoa" espelha a tabela dbo.Pessoas do banco CrudConsoleDB
// (o MESMO banco usado no projeto de ontem):
//
//   CREATE TABLE Pessoas (
//       Id    INT IDENTITY PRIMARY KEY,   -> propriedade Id
//       Nome  NVARCHAR(100) NOT NULL,     -> propriedade Nome
//       Idade INT NOT NULL                -> propriedade Idade
//   )
//
// IMPORTANTE: aqui NÃO há Entity Framework. Ninguém vai transformar as
// linhas do banco em objetos automaticamente — quem faz isso somos NÓS,
// lá na classe PessoaDAO, lendo coluna por coluna com o SqlDataReader.
// Por isso esta classe é só um "pacote de dados" com regras de validação.
// =====================================================================

using System.ComponentModel.DataAnnotations; // Atributos de validação ([Required], [Range]...)

namespace CrudPessoasAdoNet.Models
{
    public class Pessoa
    {
        // Chave primária. Como a coluna é IDENTITY, o SQL Server gera o valor
        // sozinho no INSERT — nunca enviamos o Id ao inserir.
        public int Id { get; set; }

        // [Required]          -> campo obrigatório (equivale ao NOT NULL).
        // [StringLength(100)] -> no máximo 100 caracteres (igual ao NVARCHAR(100)).
        // [Display]           -> texto exibido nos rótulos (labels) das telas.
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        // [Range] -> limita os valores aceitos (evita idades negativas ou absurdas).
        [Required(ErrorMessage = "A idade é obrigatória.")]
        [Range(0, 150, ErrorMessage = "A idade deve estar entre 0 e 150.")]
        [Display(Name = "Idade")]
        public int Idade { get; set; }
    }
}
