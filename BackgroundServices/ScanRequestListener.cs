using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.SecurityGate.Service.Dtos;
using Sentinel.SecurityGate.Service.Services;

namespace Sentinel.SecurityGate.Service.BackgroundServices;

public class ScanRequestListener : BackgroundService
{
    private readonly ILogger<ScanRequestListener> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ScanRequestListener(ILogger<ScanRequestListener> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScanRequestListener iniciado");

        using var scope = _serviceProvider.CreateScope();
        var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

        rabbit.StartListeningForRequests(async (message) =>
        {
            await ProcessMessageAsync(message, scope.ServiceProvider);
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(string message, IServiceProvider serviceProvider)
    {
        try
        {
            _logger.LogInformation("Procesando mensaje de solicitud de escaneo: {Message}", message);

            // Intentar mapear el payload a ScanCommandDto
            var jsonDoc = JsonDocument.Parse(message);

            var cmd = new ScanCommandDto { ScanType = "SAST" };

            if (jsonDoc.RootElement.TryGetProperty("scanId", out var scanIdEl) && scanIdEl.ValueKind == JsonValueKind.String)
            {
                if (Guid.TryParse(scanIdEl.GetString(), out var g)) cmd.ScanId = g;
            }

            if (jsonDoc.RootElement.TryGetProperty("requestedService", out var reqSvc))
            {
                cmd.ScanType = reqSvc.GetString() ?? "SAST";
            }

            if (jsonDoc.RootElement.TryGetProperty("targetRepo", out var repo)) cmd.RepositoryUrl = repo.GetString();
            if (jsonDoc.RootElement.TryGetProperty("targetUrl", out var turl)) cmd.TargetUrl = turl.GetString();
            if (jsonDoc.RootElement.TryGetProperty("clientGitToken", out var token)) cmd.ClientGitToken = token.GetString();

            // Iniciar workflow (llama a n8n vía HttpScanOrchestrator)
            var orchestrator = serviceProvider.GetRequiredService<IScanOrchestrator>();
            await orchestrator.StartScanWorkflowAsync(cmd);

            _logger.LogInformation("Workflow iniciado para ScanId: {ScanId}", cmd.ScanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando solicitud de escaneo desde RabbitMQ");
        }
    }
}
