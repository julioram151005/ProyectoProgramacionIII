using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using ProyectoProgramacionIII.Data;
using ProyectoProgramacionIII.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔧 FORZAR la cadena de conexión directamente
var connectionString = "Host=ep-dry-voice-aqdx5907.c-8.us-east-1.aws.neon.tech; Database=neondb; Username=neondb_owner; Password=npg_YW0JDOzkAq9v; SSL Mode=Require; Trust Server Certificate=true;";

// Agregar servicios
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Configurar límites de tamaño de archivo
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

// Configurar Entity Framework con la cadena DIRECTA
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsqlOptions => {
            npgsqlOptions.CommandTimeout(60);
            npgsqlOptions.EnableRetryOnFailure(3);
        }));

var app = builder.Build();

// Prueba de conexión
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        bool canConnect = dbContext.Database.CanConnectAsync().GetAwaiter().GetResult();
        Console.WriteLine("✅ Conexión exitosa a Neon.tech!");
        int count = dbContext.Archivos.Count();
        Console.WriteLine($"📋 Tabla Archivos: Existe (registros: {count})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine("📋 Posiblemente la tabla Archivos no existe.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();