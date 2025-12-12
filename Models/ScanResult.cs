using System;
using System.Collections.Generic;

namespace Sentinel.SecurityGate.Service.Models
{
    /// <summary>
    /// Resultado completo de un escaneo de seguridad
    /// </summary>
    public class ScanResult
    {
        public Guid ScanId { get; set; }
        
        public string? ScanType { get; set; }
        
        public string? Target { get; set; }
        
        public string? ClientId { get; set; }
        
        /// <summary>
        /// Timestamp de cuando inició el escaneo
        /// </summary>
        public DateTime StartedAt { get; set; }
        
        /// <summary>
        /// Timestamp de cuando finalizó el escaneo
        /// </summary>
        public DateTime CompletedAt { get; set; }
        
        /// <summary>
        /// Duración total del escaneo
        /// </summary>
        public TimeSpan Duration => CompletedAt - StartedAt;
        
        /// <summary>
        /// Resultado del Quality Gate (true = aprobado, false = fallido)
        /// </summary>
        public bool QualityGatePassed { get; set; }
        
        /// <summary>
        /// Lista de hallazgos de seguridad encontrados
        /// </summary>
        public List<Finding> Findings { get; set; } = new();
        
        /// <summary>
        /// Resumen de severidades
        /// </summary>
        public SeveritySummary Summary { get; set; } = new();
        
        /// <summary>
        /// Ruta al archivo raw de resultados (JSON, XML, etc.)
        /// </summary>
        public string? RawResultsPath { get; set; }
        
        /// <summary>
        /// Metadata adicional del escaneo
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
        
        /// <summary>
        /// Estado final del escaneo
        /// </summary>
        public ScanStatus Status { get; set; }
        
        /// <summary>
        /// Mensaje de error si el escaneo falló
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
    
    public class SeveritySummary
    {
        public int Critical { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public int Info { get; set; }
        
        public int Total => Critical + High + Medium + Low + Info;
    }
    
    public enum ScanStatus
    {
        Completed,
        CompletedWithErrors,
        Failed,
        Timeout
    }
}