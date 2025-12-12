using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.SecurityGate.Service.Models
{
    public class Scan
    {
    // Identificador único de esta ejecución de escaneo
    public Guid ScanId { get; set; }

    // Tipo de escaneo (SAST, DAST, PORTS_SCAN, etc.)
    public required string ScanType { get; set; }

    // El objetivo del escaneo (URL o Repositorio)
    public required string Target { get; set; }

    // Hora en que terminó el escaneo
    public DateTime TimestampUtc { get; set; }

    // Resultado de la Puerta de Calidad (True = Pasa, False = Falla)
    public bool QualityGatePassed { get; set; }

    // La ruta local donde el archivo de resultados original fue guardado por el Orquestador
    public string? RawFilePath { get; set; }

    // Una lista estandarizada de todos los hallazgos reportados por el scanner
    public List<Finding> Findings { get; set; } = new List<Finding>();
    }
}