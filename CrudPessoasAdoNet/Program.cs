// =====================================================================
// Program.cs — PONTO DE PARTIDA da aplicação
// ---------------------------------------------------------------------
// Aqui (1) registramos os SERVIÇOS que a aplicação usa e
//      (2) configuramos o PIPELINE (o caminho que cada requisição percorre).
//
// DIFERENÇA para o projeto de ontem (CrudPessoasMvc):
//   Lá registrávamos um DbContext do Entity Framework (AddDbContext).
//   Aqui NÃO existe Entity Framework: registramos a nossa própria classe
//   PessoaDAO, que fala com o banco usando SqlConnection + SQL "na mão".
// =====================================================================

using CrudPessoasAdoNet.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1) REGISTRO DE SERVIÇOS (Injeção de Dependência)
// ---------------------------------------------------------------------

// Habilita o padrão MVC: Controllers + Views.
builder.Services.AddControllersWithViews();

// Registra a classe PessoaDAO (Data Access Object = objeto de acesso a dados).
// AddScoped = uma instância nova a cada requisição HTTP.
// Assim, o PessoasController pode pedir um PessoaDAO no construtor
// e o ASP.NET entrega pronto.
builder.Services.AddScoped<PessoaDAO>();

var app = builder.Build();

// ---------------------------------------------------------------------
// 2) PIPELINE DE REQUISIÇÕES (middlewares, executados em ordem)
// ---------------------------------------------------------------------

// Em produção, erros vão para uma página amigável em vez de mostrar detalhes técnicos.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Redireciona HTTP -> HTTPS
app.UseStaticFiles();      // Permite servir arquivos da pasta wwwroot (CSS, JS, imagens)

app.UseRouting();          // Descobre qual Controller/Action atende a URL

app.UseAuthorization();

// Rota padrão: /{controller}/{action}/{id?}
// "Pessoas/Index" é a página inicial: o site já abre na listagem do CRUD.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pessoas}/{action=Index}/{id?}");

app.Run(); // Inicia o servidor web
