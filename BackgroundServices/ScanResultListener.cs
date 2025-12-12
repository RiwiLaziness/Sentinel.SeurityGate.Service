using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.SecurityGate.Service.Models;
using Sentinel.SecurityGate.Service.Services;

namespace Sentinel.SecurityGate.Service.BackgroundServices
{
    /// <summary>
    /// Servicio en background que escucha los resultados de escaneo desde RabbitMQ
    /// </summary>
    public class ScanResultListener : BackgroundService
    {
        private readonly ILogger<ScanResultListener> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ScanResultListener(
            ILogger<ScanResultListener> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScanResultListener iniciado");

            using var scope = _serviceProvider.CreateScope();
            var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

            // Iniciar el listener de RabbitMQ
            rabbitMqService.StartListeningForResults(async (message) =>
            {
                await ProcessScanResultAsync(message, scope.ServiceProvider);
            });

            // Mantener el servicio corriendo
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessScanResultAsync(string message, IServiceProvider serviceProvider)
        {
            try
            {
                // Deserializar el resultado del escaneo
                var scanResult = JsonSerializer.Deserialize<ScanResult>(message);
                
                if (scanResult == null)
                {
                    _logger.LogWarning("Mensaje recibido no pudo ser deserializado");
                    return;
                }

                _logger.LogInformation(
                    "Resultado de escaneo recibido. ScanId: {ScanId}, Status: {Status}, QualityGate: {QualityGate}",
                    scanResult.ScanId,
                    scanResult.Status,
                    scanResult.QualityGatePassed);

                var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var bffWebhookUrl = configuration["JavaBFF:WebhookUrl"];

                if (!string.IsNullOrEmpty(bffWebhookUrl))
                {
                    var notification = new
                    {
                        scanId = scanResult.ScanId,
                        status = scanResult.Status.ToString(),
                        qualityGatePassed = scanResult.QualityGatePassed,
                        completedAt = scanResult.CompletedAt,
                        summary = scanResult.Summary
                    };

                    var jsonContent = new System.Net.Http.StringContent(
                        JsonSerializer.Serialize(notification),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    var response = await httpClient.PostAsync(bffWebhookUrl, jsonContent);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Notificación enviada al Java BFF para ScanId: {ScanId}", 
                            scanResult.ScanId);
                    }
                    else
                    {
                        _logger.LogWarning("Error al notificar al Java BFF. Status: {StatusCode}", 
                            response.StatusCode);
                    }
                }

                _logger.LogInformation("Resultado procesado exitosamente para ScanId: {ScanId}", 
                    scanResult.ScanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando resultado de escaneo");
                throw; // Para que RabbitMQ reencole el mensaje
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ScanResultListener detenido");
            await base.StopAsync(cancellationToken);
        }
    }
}
