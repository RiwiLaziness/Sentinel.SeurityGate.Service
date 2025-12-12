using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.SecurityGate.Service.Dtos
{

    public class ScanAcceptanceDto
    {
        /// <summary>
        /// Identificador único generado por el sistema para esta solicitud de escaneo.
        /// Este ID debe ser usado por el Java BFF para consultar el estado posteriormente.
        /// </summary>
        public Guid ScanId { get; set; }

        /// <summary>
        /// Estado inicial de la tarea. Siempre debe ser "ACCEPTED", "PENDING" o "INITIALIZED".
        /// </summary>
        public string Status { get; set; } = "ACCEPTED";

        /// <summary>
        /// El tipo de servicio solicitado (ej. "FULL_SAST", "DAST_BASIC"). 
        /// Útil para la trazabilidad y logs.
        /// </summary>
        public string RequestedService { get; set; }

        /// <summary>
        /// Marca de tiempo de cuándo el SecurityGate recibió y aceptó la solicitud.
        /// </summary>
        public DateTime AcceptanceTimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// (Opcional) Indica el método que se usará para notificar la finalización. 
        /// En este caso, RabbitMQ o Webhook al Java BFF.
        /// </summary>
        public string CompletionMethod { get; set; } = "RABBITMQ_EVENT";
    }
}