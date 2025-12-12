using Microsoft.AspNetCore.Mvc;
using Sentinel.SecurityGate.Service.Dtos;
using Sentinel.SecurityGate.Service.Services;

namespace Sentinel.SecurityGate.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly ILogger<ScanController> _logger;
        private readonly IScanOrchestrator _orchestrator;
        private readonly IRabbitMqService _rabbitMqService;

        public ScanController(
            ILogger<ScanController> logger,
            IScanOrchestrator orchestrator,
            IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _orchestrator = orchestrator;
            _rabbitMqService = rabbitMqService;
        }

        /// <summary>
        /// Endpoint para solicitar un nuevo escaneo
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(typeof(ScanAcceptanceDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestScan([FromBody] ScanCommandDto command)
        {
            try
            {
                // Validar campos obligatorios según tipo de escaneo
                var validationError = ValidateScanCommand(command);
                if (validationError != null)
                {
                    return BadRequest(new { error = validationError });
                }

                var target = command.ScanType.ToUpperInvariant() switch
                {
                    "SAST" or "FULL_SAST" => command.RepositoryUrl,
                    "DAST" or "DAST_BASIC" => command.TargetUrl,
                    _ => command.RepositoryUrl ?? command.TargetUrl
                };

                _logger.LogInformation(
                    "Solicitud de escaneo recibida: {ScanType} para {Target}",
                    command.ScanType,
                    target);

                // Generar ScanId si no viene
                if (command.ScanId == Guid.Empty)
                {
                    command.ScanId = Guid.NewGuid();
                }

                // Iniciar workflow en n8n
                var scanId = await _orchestrator.StartScanWorkflowAsync(command);

                // Publicar en RabbitMQ para que otros servicios lo procesen
                var routingKey = GetRoutingKeyForScanType(command.ScanType);
                await _rabbitMqService.PublishScanRequestAsync(command, routingKey);

                // Responder inmediatamente con aceptación
                var acceptance = new ScanAcceptanceDto
                {
                    ScanId = scanId,
                    Status = "ACCEPTED",
                    RequestedService = command.ScanType,
                    AcceptanceTimestampUtc = DateTime.UtcNow,
                    CompletionMethod = "RABBITMQ_EVENT"
                };

                return AcceptedAtAction(
                    nameof(GetScanStatus),
                    new { scanId },
                    acceptance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando solicitud de escaneo");
                return BadRequest(new
                {
                    error = "Error procesando solicitud de escaneo",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint para consultar el estado de un escaneo
        /// </summary>
        [HttpGet("{scanId}/status")]
        [ProducesResponseType(typeof(ScanStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetScanStatus(Guid scanId)
        {
            try
            {
                // Aquí deberías consultar el estado desde tu DB o cache
                // Por ahora retornamos un ejemplo
                
                var status = new ScanStatusDto
                {
                    ScanId = scanId,
                    Status = "IN_PROGRESS",
                    ScanType = "SAST",
                    Target = "https://github.com/example/repo",
                    StartedAt = DateTime.UtcNow.AddMinutes(-5),
                    ProgressPercentage = 45,
                    StatusMessage = "Analizando código fuente...",
                    CurrentStep = "code_analysis"
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando estado de escaneo {ScanId}", scanId);
                return NotFound(new { error = $"Escaneo {scanId} no encontrado" });
            }
        }

        /// <summary>
        /// Endpoint webhook para recibir resultados de n8n
        /// </summary>
        [HttpPost("webhook/result")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReceiveScanResult([FromBody] object scanResult)
        {
            try
            {
                _logger.LogInformation("Resultado de escaneo recibido desde n8n");

                // Publicar el resultado en RabbitMQ
                await _rabbitMqService.PublishScanResultAsync(scanResult);

                return Ok(new { message = "Resultado recibido y procesado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando resultado de escaneo");
                return StatusCode(500, new { error = ex.Message                 });
            }
        }

        /// <summary>
        /// Valida que el comando tenga todos los campos obligatorios según el tipo de escaneo
        /// </summary>
        private string? ValidateScanCommand(ScanCommandDto command)
        {
            switch (command.ScanType.ToUpperInvariant())
            {
                case "SAST":
                case "FULL_SAST":
                    if (string.IsNullOrWhiteSpace(command.RepositoryUrl))
                        return "El campo 'RepositoryUrl' es obligatorio para escaneos SAST";
                    if (string.IsNullOrWhiteSpace(command.Branch))
                        return "El campo 'Branch' es obligatorio para escaneos SAST";
                    break;

                case "DAST":
                case "DAST_BASIC":
                    if (string.IsNullOrWhiteSpace(command.TargetUrl))
                        return "El campo 'TargetUrl' es obligatorio para escaneos DAST";
                    break;
            }

            return null;
        }

        private string GetRoutingKeyForScanType(string scanType)
        {
            return scanType.ToUpperInvariant() switch
            {
                "FULL_SAST" or "SAST" => "scan.sast",
                "DAST_BASIC" or "DAST" => "scan.dast",
                "PORTS_SCAN" => "scan.ports",
                "SECRETS_SCAN" => "scan.secrets",
                _ => $"scan.{scanType.ToLower()}"
            };
        }
    }
}