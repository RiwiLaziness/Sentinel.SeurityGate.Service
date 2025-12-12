using Sentinel.SecurityGate.Service.Dtos;

public interface IScanOrchestrator
{
    // Inicia el flujo en n8n y devuelve el ID de la tarea aceptada.
    Task<Guid> StartScanWorkflowAsync(ScanCommandDto command);
}