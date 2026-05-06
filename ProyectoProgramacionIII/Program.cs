using Microsoft.AspNetCore.Http.Features;
using ProyectoProgramacionIII.Models;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios para MVC (vistas)
builder.Services.AddControllersWithViews();

// Agregar servicios para API controllers
builder.Services.AddControllers();

// Configurar la ruta base donde se guardarán los archivos
builder.Services.Configure<ArchiveSettings>(builder.Configuration.GetSection("ArchiveSettings"));

// ✅ Configurar límites de tamaño de archivo (ANTES de Build)
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

// Mapear rutas de controladores MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Mapear rutas de API
app.MapControllers();

app.Run();