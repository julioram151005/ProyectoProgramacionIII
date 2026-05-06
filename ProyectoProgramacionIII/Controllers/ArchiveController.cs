using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoProgramacionIII.Data;
using ProyectoProgramacionIII.Models;
using System.Security.Cryptography;

namespace ProyectoProgramacionIII.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
    private readonly AppDbContext _context;

    public ArchiveController(AppDbContext context)
    {
        _context = context;
    }

    // 📤 SUBIR archivo (almacena en BD como BYTEA)
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        Check.FileUpload(file);

        // Leer el archivo como bytes
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var contenido = memoryStream.ToArray();

        // Calcular MD5
        var hash = MD5.HashData(contenido);
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        // Guardar en la base de datos
        var archivo = new Archivo
        {
            NombreOriginal = file.FileName,
            Contenido = contenido,
            HashMd5 = hashString,
            TamanoBytes = file.Length,
            TipoMime = file.ContentType ?? "application/zip"
        };

        _context.Archivos.Add(archivo);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Archivo subido correctamente",
            id = archivo.Id,
            nombre = archivo.NombreOriginal,
            tamano = archivo.TamanoBytes,
            hash = archivo.HashMd5
        });
    }

    // 📋 LISTAR archivos (solo metadatos, sin contenido)
    [HttpGet("list")]
    public async Task<IActionResult> ListFiles()
    {
        var archivos = await _context.Archivos
            .Select(a => new {
                a.Id,
                a.NombreOriginal,
                a.TamanoBytes,
                a.HashMd5,
                a.TipoMime,
                a.FechaSubida
            })
            .ToListAsync();

        return Ok(archivos);
    }

    // 📥 DESCARGAR archivo por ID
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var archivo = await _context.Archivos.FindAsync(id);
        if (archivo == null)
            return NotFound("Archivo no encontrado");

        return File(archivo.Contenido, archivo.TipoMime, archivo.NombreOriginal);
    }

    // 🗑️ ELIMINAR archivo
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var archivo = await _context.Archivos.FindAsync(id);
        if (archivo == null)
            return NotFound("Archivo no encontrado");

        _context.Archivos.Remove(archivo);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Archivo eliminado correctamente" });
    }

    // 🔍 OBTENER información de un archivo
    [HttpGet("info/{id}")]
    public async Task<IActionResult> GetInfo(int id)
    {
        var archivo = await _context.Archivos
            .Where(a => a.Id == id)
            .Select(a => new { a.Id, a.NombreOriginal, a.TamanoBytes, a.HashMd5, a.TipoMime, a.FechaSubida })
            .FirstOrDefaultAsync();

        if (archivo == null)
            return NotFound("Archivo no encontrado");

        return Ok(archivo);
    }

    [HttpGet("test-db")]
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            // Probar conexión
            var canConnect = await _context.Database.CanConnectAsync();

            // Contar archivos
            var count = await _context.Archivos.CountAsync();

            return Ok(new
            {
                connected = canConnect,
                archivosCount = count,
                message = "Conexión exitosa a Neon.tech"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Clase auxiliar para validaciones
public static class Check
{
    public static void FileUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se recibió ningún archivo.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".zip" && extension != ".rar")
            throw new ArgumentException("Solo se permiten archivos .zip o .rar");

        if (file.Length > 100 * 1024 * 1024) // 100 MB
            throw new ArgumentException("El archivo no puede superar los 100 MB");
    }
}
