using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.SecurityGate.Service.Dtos
{
    /// <summary>
    /// DTO para consultar el estado de un escaneo en progreso
    /// </summary>
    public class ScanStatusDto
    {
        public Guid ScanId { get; set; }
        
        public string Status { get; set; } // PENDING, IN_PROGRESS, COMPLETED, FAILED
        
        public string ScanType { get; set; }
        
        public string Target { get; set; }
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Porcentaje de progreso estimado (0-100)
        /// </summary>
        public int? ProgressPercentage { get; set; }
        
        /// <summary>
        /// Mensaje descriptivo del estado actual
        /// </summary>
        public string? StatusMessage { get; set; }
        
        /// <summary>
        /// Paso actual del workflow en n8n
        /// </summary>
        public string? CurrentStep { get; set; }
    }
}