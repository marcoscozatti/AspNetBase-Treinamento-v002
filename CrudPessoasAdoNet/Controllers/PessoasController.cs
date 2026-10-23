// =====================================================================
// CONTROLLER (o "C" do MVC)
// ---------------------------------------------------------------------
// O Controller recebe as requisições do navegador, pede os dados ao
// PessoaDAO (que executa o SQL) e escolhe qual View (tela) mostrar.
//
// Repare que o Controller NÃO tem nenhum SQL: ele só chama métodos do DAO.
// Cada camada tem a sua responsabilidade:
//   View       -> mostra a tela
//   Controller -> decide o que fazer com cada requisição
//   DAO        -> conversa com o banco (SELECT, INSERT, UPDATE, DELETE)
//
// Convenção de rotas (definida no Program.cs):
//   /{controller}/{action}/{id?}
//   Ex.: /Pessoas/Edit/3 -> Controller "Pessoas", Action "Edit", id = 3
// =====================================================================

using Microsoft.AspNetCore.Mvc;
using CrudPessoasAdoNet.Data;
using CrudPessoasAdoNet.Models;

namespace CrudPessoasAdoNet.Controllers
{
    public class PessoasController : Controller
    {
        // Campo que guarda o DAO (nosso acesso ao banco).
        private readonly PessoaDAO _dao;

        // INJEÇÃO DE DEPENDÊNCIA:
        // Não criamos o PessoaDAO com "new". O ASP.NET cria e nos entrega
        // pronto, porque registramos ele no Program.cs (AddScoped).
        public PessoasController(PessoaDAO dao)
        {
            _dao = dao;
        }

        // =============================================================
        // READ (Listar) — GET: /Pessoas
        // SQL executado: SELECT Id, Nome, Idade FROM Pessoas ORDER BY Nome
        // =============================================================
        public IActionResult Index()
        {
            List<Pessoa> pessoas = _dao.Listar();
            return View(pessoas); // Envia a lista para Views/Pessoas/Index.cshtml
        }

        // =============================================================
        // READ (Detalhes) — GET: /Pessoas/Details/5
        // SQL executado: SELECT ... FROM Pessoas WHERE Id = @Id
        // =============================================================
        public IActionResult Details(int id)
        {
            Pessoa? pessoa = _dao.BuscarPorId(id);

            // Se não existe pessoa com esse id, devolvemos erro 404.
            if (pessoa == null)
            {
                return NotFound();
            }

            return View(pessoa);
        }

        // =============================================================
        // CREATE (Criar) — parte 1: GET /Pessoas/Create
        // Apenas MOSTRA o formulário vazio (não acessa o banco).
        // =============================================================
        public IActionResult Create()
        {
            return View();
        }

        // =============================================================
        // CREATE (Criar) — parte 2: POST /Pessoas/Create
        // SQL executado: INSERT INTO Pessoas (Nome, Idade) VALUES (@Nome, @Idade)
        // =============================================================
        // [HttpPost] -> só responde a envios de formulário (POST).
        // [ValidateAntiForgeryToken] -> proteção contra ataques CSRF.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pessoa pessoa)
        {
            // ModelState.IsValid verifica as regras do Model ([Required], [Range]...).
            // Se algo estiver errado, voltamos ao formulário mostrando os erros.
            if (!ModelState.IsValid)
            {
                return View(pessoa);
            }

            int novoId = _dao.Inserir(pessoa);

            // TempData guarda uma mensagem que sobrevive a UM redirecionamento.
            TempData["Mensagem"] = $"Pessoa cadastrada com sucesso! (Id gerado: {novoId})";

            // Padrão PRG (Post-Redirect-Get): evita que um F5 duplique o INSERT.
            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        // UPDATE (Editar) — parte 1: GET /Pessoas/Edit/5
        // Busca a pessoa (SELECT) e mostra o formulário PREENCHIDO.
        // =============================================================
        public IActionResult Edit(int id)
        {
            Pessoa? pessoa = _dao.BuscarPorId(id);
            if (pessoa == null)
            {
                return NotFound();
            }

            return View(pessoa);
        }

        // =============================================================
        // UPDATE (Editar) — parte 2: POST /Pessoas/Edit/5
        // SQL executado: UPDATE Pessoas SET Nome=@Nome, Idade=@Idade WHERE Id=@Id
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pessoa pessoa)
        {
            // Segurança: o id da URL deve ser o mesmo id do formulário.
            if (id != pessoa.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(pessoa);
            }

            // Atualizar() devolve false se nenhuma linha foi alterada,
            // ou seja, a pessoa foi excluída por outro usuário nesse meio-tempo.
            bool atualizou = _dao.Atualizar(pessoa);
            if (!atualizou)
            {
                return NotFound();
            }

            TempData["Mensagem"] = "Pessoa atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        // DELETE (Excluir) — parte 1: GET /Pessoas/Delete/5
        // Mostra uma tela de CONFIRMAÇÃO (nunca apagamos direto num GET).
        // =============================================================
        public IActionResult Delete(int id)
        {
            Pessoa? pessoa = _dao.BuscarPorId(id);
            if (pessoa == null)
            {
                return NotFound();
            }

            return View(pessoa);
        }

        // =============================================================
        // DELETE (Excluir) — parte 2: POST /Pessoas/Delete/5
        // SQL executado: DELETE FROM Pessoas WHERE Id = @Id
        // =============================================================
        // [ActionName("Delete")] -> em C# não podem existir dois métodos
        // Delete(int id) iguais, então este se chama DeleteConfirmed,
        // mas para o MVC ele continua sendo a action "Delete".
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_dao.Excluir(id))
            {
                TempData["Mensagem"] = "Pessoa excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
