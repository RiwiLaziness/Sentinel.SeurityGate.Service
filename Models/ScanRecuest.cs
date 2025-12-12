using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.SecurityGate.Service.Models
{
    /// <summary>
    /// Representa una solicitud de escaneo recibida desde el Java BFF
    /// </summary>
    public class ScanRequest
    {
        public Guid ScanId { get; set; }
        
        /// <summary>
        /// Tipo de escaneo solicitado (FULL_SAST, DAST_BASIC, PORTS_SCAN, SECRETS_SCAN, etc.)
        /// </summary>
        public required string ScanType { get; set; }
        
        /// <summary>
        /// URL del repositorio o aplicación a escanear
        /// </summary>
        public required string Target { get; set; }
        
        /// <summary>
        /// Token de acceso al repositorio Git (si aplica)
        /// </summary>
        public string? ClientGitToken { get; set; }
        
        /// <summary>
        /// ID del cliente/organización que solicita el escaneo
        /// </summary>
        public required string ClientId { get; set; }
        
        /// <summary>
        /// Configuración adicional específica del tipo de escaneo
        /// </summary>
        public Dictionary<string, string>? ScanConfiguration { get; set; }
        
        /// <summary>
        /// Timestamp de cuando se recibió la solicitud
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Estado actual de la solicitud
        /// </summary>
        public ScanRequestStatus Status { get; set; } = ScanRequestStatus.Pending;
        
        /// <summary>
        /// URL de callback o webhook para notificar resultados (opcional)
        /// </summary>
        public string? CallbackUrl { get; set; }
    }
    
    public enum ScanRequestStatus
    {
        Pending,
        Accepted,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
}