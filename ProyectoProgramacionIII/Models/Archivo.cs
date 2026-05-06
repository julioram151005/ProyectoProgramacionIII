using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoProgramacionIII.Models;

[Table("Archivos")]  // ← Nombre exacto como en la BD
public class Archivo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required]
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    public string HashMd5 { get; set; } = string.Empty;

    public long TamanoBytes { get; set; }

    public string TipoMime { get; set; } = "application/zip";

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}