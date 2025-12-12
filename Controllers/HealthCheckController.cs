using Microsoft.AspNetCore.Mvc;

namespace Sentinel.SecurityGate.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check solicitado");
        return Ok(new
        {
            status = "Healthy",
            service = "Sentinel.SecurityGate.Service",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Aquí puedes agregar verificaciones adicionales (DB, RabbitMQ, etc.)
        return Ok(new
        {
            status = "Ready",
            timestamp = DateTime.UtcNow
        });
    }
}