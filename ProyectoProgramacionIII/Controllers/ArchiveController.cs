using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProyectoProgramacionIII.Models;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace ProyectoProgramacionIII.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
    private readonly string _baseDir;

    public ArchiveController(IOptions<ArchiveSettings> settings)
    {
        _baseDir = Path.GetFullPath(settings.Value.BaseDirectory);
        if (!Directory.Exists(_baseDir))
            Directory.CreateDirectory(_baseDir);
    }

    // READY: Subir archivo comprimido
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        Check.FileUpload(file);

        var fullPath = Path.Combine(_baseDir, file!.FileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        return Ok(new { message = "Archivo subido correctamente", path = fullPath });
    }

    // READY: Listar contenidos
    [HttpGet("contents")]
    public IActionResult GetContents(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("Debe indicar el nombre del archivo");

        var fullPath = Path.Combine(_baseDir, fileName);
        if (!System.IO.File.Exists(fullPath))
            return NotFound("El archivo no existe");

        try
        {
            using var archive = ArchiveFactory.OpenArchive(fullPath);
            var entries = archive.Entries
                .Where(e => !e.IsDirectory)
                .Select(e => new
                {
                    e.Key,
                    Size = e.Size,
                    CompressedSize = e.CompressedSize,
                    LastModified = e.LastModifiedTime
                })
                .ToList();

            return Ok(entries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al leer el archivo: {ex.Message}");
        }
    }

    // READY: Extraer archivo comprimido usando WriteToDirectory
    [HttpPost("extract")]
    public IActionResult Extract(string fileName, string? outputFolder = null)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("Debe indicar el nombre del archivo");

        var archivePath = Path.Combine(_baseDir, fileName);
        if (!System.IO.File.Exists(archivePath))
            return NotFound("El archivo no existe");

        var extractDir = string.IsNullOrEmpty(outputFolder)
            ? Path.Combine(_baseDir, Path.GetFileNameWithoutExtension(fileName))
            : Path.Combine(_baseDir, outputFolder);

        try
        {
            using var archive = ArchiveFactory.OpenArchive(archivePath);
            // ✅ CORRECCIÓN: Usar WriteToDirectory en lugar de ExtractToDirectory
            archive.WriteToDirectory(extractDir, new ExtractionOptions()
            {
                ExtractFullPath = true,
                Overwrite = true
            });

            return Ok(new { message = "Extracción completada", destination = extractDir });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al extraer: {ex.Message}");
        }
    }

    // READY: Comprimir a ZIP
    [HttpPost("compress")]
    public IActionResult Compress(string sourcePath, string zipName)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(zipName))
            return BadRequest("Debe indicar sourcePath y zipName");

        var sourceFull = Path.Combine(_baseDir, sourcePath);
        if (!Directory.Exists(sourceFull) && !System.IO.File.Exists(sourceFull))
            return NotFound("El origen no existe");

        if (!zipName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            zipName += ".zip";

        var zipFull = Path.Combine(_baseDir, zipName);

        try
        {
            using var stream = System.IO.File.OpenWrite(zipFull);
            // ✅ CORRECCIÓN: Usar WriterFactory.Open con los parámetros correctos
            using var writer = WriterFactory.OpenWriter(stream, ArchiveType.Zip, new WriterOptions(CompressionType.Deflate));
            if (Directory.Exists(sourceFull))
                writer.WriteAll(sourceFull, "*", SearchOption.AllDirectories);
            else
                writer.Write(Path.GetFileName(sourceFull), sourceFull);

            return Ok(new { message = "Compresión completada", zipPath = zipFull });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al comprimir: {ex.Message}");
        }
    }

    // READY: Método estándar para descarga... sin cambios
    [HttpGet("download")]
    public IActionResult Download(string filePath)
    {
        try
        {
            var fullPath = GetSafePath(filePath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();
            return PhysicalFile(fullPath, "application/octet-stream", Path.GetFileName(fullPath));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // READY: Eliminar archivo o carpeta
    [HttpDelete("delete")]
    public IActionResult Delete(string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Debe indicar la ruta relativa");

        var fullPath = Path.Combine(_baseDir, path);
        if (!System.IO.File.Exists(fullPath) && !Directory.Exists(fullPath))
            return NotFound("El elemento no existe");

        try
        {
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, recursive: true);
            else
                System.IO.File.Delete(fullPath);

            return Ok(new { message = "Elemento eliminado correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar: {ex.Message}");
        }
    }

    // READY: Listar archivos
    [HttpGet("list")]
    public IActionResult ListFiles()
    {
        var items = Directory.GetFileSystemEntries(_baseDir)
            .Select(entry => new
            {
                Name = Path.GetRelativePath(_baseDir, entry),
                IsDirectory = Directory.Exists(entry),
                FullPath = entry
            });
        return Ok(items);
    }

    private string GetSafePath(string relativePath)
    {
        var full = Path.GetFullPath(Path.Combine(_baseDir, relativePath));
        if (!full.StartsWith(_baseDir, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Acceso fuera del directorio permitido");
        return full;
    }
}

// Clase auxiliar static para validaciones repetitivas
public static class Check
{
    public static void FileUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se recibió ningún archivo.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".zip" && extension != ".rar")
            throw new ArgumentException("Solo se permiten archivos .zip o .rar");
    }
}