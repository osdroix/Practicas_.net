using TaskManager.Web.Extensions;
using TaskManager.Web.Services;
using TaskManager.Web.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApiClients();
builder.Services.AddBusinessServices();

// Registra el HttpClient con el manejador de excepciones
builder.Services.AddTransient<ApiExceptionHandler>();

builder.Services.AddHttpClient("TaskManagerApi")
    .AddHttpMessageHandler<ApiExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMvcGlobalErrorHandler();


app.UseAuthorization();

app.MapControllers();

//Esa l�nea define la ruta �cl�sica� (MVC) que ASP.NET Core usa para decidir qu� controlador y qu� acci�n ejecutar cuando llega una petici�n HTTP. Si llega una URL y no coincide con nada m�s espec�fico,intenta interpretarla como:Controlador / Acci�n / Id opcional.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
