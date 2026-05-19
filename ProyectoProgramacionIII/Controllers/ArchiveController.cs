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
    private static ArbolBinario arbolArchivos = new ArbolBinario();

    public ArchiveController(AppDbContext context)
    {
        _context = context;
    }

    // 📤 SUBIR archivo (almacena en BD como BYTEA)
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        Check.FileUpload(file);

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var contenido = memoryStream.ToArray();

        var hash = MD5.HashData(contenido);
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

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
            var canConnect = await _context.Database.CanConnectAsync();
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

    // Método privado para cargar el árbol desde la base de datos
    private async Task CargarArbolDesdeBD()
    {
        arbolArchivos = new ArbolBinario();

        var ids = await _context.Archivos
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync();

        foreach (var id in ids)
        {
            arbolArchivos.Insertar(id);
        }
    }

    // 🌳 RECORRIDOS DEL ÁRBOL (devuelven datos reales de la BD)
    [HttpGet("arbol/preorden")]
    public async Task<IActionResult> RecorridoPreOrden()
    {
        await CargarArbolDesdeBD();

        var idsRecorrido = arbolArchivos.PreOrden();
        var archivosDict = await _context.Archivos
            .Where(a => idsRecorrido.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        var resultado = idsRecorrido
            .Select(id => archivosDict[id])
            .Select(a => new
            {
                a.Id,
                a.NombreOriginal,
                a.TamanoBytes,
                a.HashMd5,
                a.TipoMime,
                a.FechaSubida
            })
            .ToList();

        return Ok(new { recorrido = "PreOrden", archivos = resultado });
    }

    [HttpGet("arbol/inorden")]
    public async Task<IActionResult> RecorridoInOrden()
    {
        await CargarArbolDesdeBD();

        var idsRecorrido = arbolArchivos.InOrden();
        var archivosDict = await _context.Archivos
            .Where(a => idsRecorrido.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        var resultado = idsRecorrido
            .Select(id => archivosDict[id])
            .Select(a => new
            {
                a.Id,
                a.NombreOriginal,
                a.TamanoBytes,
                a.HashMd5,
                a.TipoMime,
                a.FechaSubida
            })
            .ToList();

        return Ok(new { recorrido = "InOrden", archivos = resultado });
    }

    [HttpGet("arbol/postorden")]
    public async Task<IActionResult> RecorridoPostOrden()
    {
        await CargarArbolDesdeBD();

        var idsRecorrido = arbolArchivos.PostOrden();
        var archivosDict = await _context.Archivos
            .Where(a => idsRecorrido.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        var resultado = idsRecorrido
            .Select(id => archivosDict[id])
            .Select(a => new
            {
                a.Id,
                a.NombreOriginal,
                a.TamanoBytes,
                a.HashMd5,
                a.TipoMime,
                a.FechaSubida
            })
            .ToList();

        return Ok(new { recorrido = "PostOrden", archivos = resultado });
    }
    [HttpDelete("arbol/eliminar/{id}")]
    public IActionResult EliminarDelArbol(int id)
    {
        // Verificar si el valor existe en el árbol (opcional, pero útil)
        var recorrido = arbolArchivos.InOrden(); // o cualquier método
        if (!recorrido.Contains(id))
            return NotFound(new { message = $"El ID {id} no está en el árbol." });

        arbolArchivos.Eliminar(id);
        return Ok(new { message = $"ID {id} eliminado del árbol." });
    }

    // (Opcional) Endpoint manual para insertar un ID en el árbol (pruebas)
    [HttpPost("arbol/insertar/{id}")]
    public IActionResult InsertarEnArbol(int id)
    {
        arbolArchivos.Insertar(id);
        return Ok(new { message = $"ID {id} insertado en el árbol" });
    }
}

// Clase auxiliar para validaciones (sin cambios)
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