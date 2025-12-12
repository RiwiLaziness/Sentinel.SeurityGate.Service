using Sentinel.SecurityGate.Service.Dtos;
using System.Text;
using System.Text.Json;

namespace Sentinel.SecurityGate.Service.Services
{
    public class HttpScanOrchestrator : IScanOrchestrator
    {
        private readonly ILogger<HttpScanOrchestrator> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HttpScanOrchestrator(
            ILogger<HttpScanOrchestrator> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<Guid> StartScanWorkflowAsync(ScanCommandDto command)
        {
            try
            {
                var scanId = command.ScanId != Guid.Empty ? command.ScanId : Guid.NewGuid();
                var n8nWebhookUrl = GetN8nWebhookUrl(command.ScanType);

                _logger.LogInformation(
                    "Iniciando workflow de {ScanType} con ScanId: {ScanId}",
                    command.ScanType,
                    scanId);

                // Crear el payload completo según el tipo de escaneo
                object payload = command.ScanType.ToUpperInvariant() switch
                {
                    "SAST" or "FULL_SAST" => CreateSastPayload(scanId, command),
                    "DAST" or "DAST_BASIC" => CreateDastPayload(scanId, command),
                    _ => CreateGenericPayload(scanId, command)
                };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(command.TimeoutMinutes ?? 30);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true 
                    }),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogDebug("Payload enviado a n8n: {Payload}", 
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

                // Trigger del workflow en n8n
                var response = await httpClient.PostAsync(n8nWebhookUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(
                        "Workflow de n8n iniciado exitosamente. ScanId: {ScanId}",
                        scanId);

                    return scanId;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al iniciar workflow en n8n. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode,
                        errorContent);
                    
                    throw new Exception($"Error al iniciar workflow en n8n: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al iniciar workflow para {ScanType}", command.ScanType);
                throw;
            }
        }

        /// <summary>
        /// Crea el payload específico para escaneos SAST (Semgrep)
        /// </summary>
        private object CreateSastPayload(Guid scanId, ScanCommandDto command)
        {
            return new
            {
                scanId = scanId,
                scanType = "SAST",
                clientId = command.ClientId,
                timestamp = DateTime.UtcNow,
                
                // Datos obligatorios para SAST
                repository = new
                {
                    url = command.RepositoryUrl,
                    branch = command.Branch ?? "main",
                    commitId = command.CommitId,
                    token = command.ClientGitToken
                },
                
                // Configuración del escaneo
                config = new
                {
                    localPath = command.LocalCodePath,
                    timeoutMinutes = command.TimeoutMinutes ?? 30,
                    additionalConfig = command.AdditionalConfig
                }
            };
        }

        /// <summary>
        /// Crea el payload específico para escaneos DAST (OWASP ZAP)
        /// </summary>
        private object CreateDastPayload(Guid scanId, ScanCommandDto command)
        {
            var payload = new
            {
                scanId = scanId,
                scanType = "DAST",
                clientId = command.ClientId,
                timestamp = DateTime.UtcNow,
                
                // URL objetivo obligatoria para DAST
                target = new
                {
                    url = command.TargetUrl,
                    scope = command.ScanScope ?? new List<string>()
                },
                
                // Autenticación (si se proporciona)
                authentication = command.Authentication != null ? new
                {
                    authType = command.Authentication.AuthType,
                    username = command.Authentication.Username,
                    password = command.Authentication.Password,
                    token = command.Authentication.Token,
                    loginUrl = command.Authentication.LoginUrl,
                    loginFormFields = command.Authentication.LoginFormFields,
                    sessionCookies = command.Authentication.SessionCookies
                } : null,
                
                // Configuración del Spider/Crawler
                spider = command.SpiderConfig != null ? new
                {
                    enabled = command.SpiderConfig.EnableSpider,
                    maxDepth = command.SpiderConfig.MaxDepth,
                    seedUrls = command.SpiderConfig.SeedUrls ?? new List<string>(),
                    excludePatterns = command.SpiderConfig.ExcludePatterns ?? new List<string>(),
                    timeoutMinutes = command.SpiderConfig.SpiderTimeoutMinutes ?? 10
                } : new
                {
                    enabled = true,
                    maxDepth = 5,
                    seedUrls = new List<string>(),
                    excludePatterns = new List<string>(),
                    timeoutMinutes = 10
                },
                
                // Configuración general
                config = new
                {
                    timeoutMinutes = command.TimeoutMinutes ?? 60,
                    additionalConfig = command.AdditionalConfig
                }
            };

            return payload;
        }

        /// <summary>
        /// Crea un payload genérico para otros tipos de escaneo
        /// </summary>
        private object CreateGenericPayload(Guid scanId, ScanCommandDto command)
        {
            return new
            {
                scanId = scanId,
                scanType = command.ScanType,
                clientId = command.ClientId,
                timestamp = DateTime.UtcNow,
                target = command.TargetUrl ?? command.RepositoryUrl,
                config = new
                {
                    timeoutMinutes = command.TimeoutMinutes ?? 30,
                    additionalConfig = command.AdditionalConfig
                }
            };
        }

        private string GetN8nWebhookUrl(string scanType)
        {
            var n8nBaseUrl = _configuration["N8n:BaseUrl"] ?? "http://localhost:5678";

            return scanType.ToUpperInvariant() switch
            {
                "FULL_SAST" or "SAST" => $"{n8nBaseUrl}/webhook/scan/sast",
                "DAST_BASIC" or "DAST" => $"{n8nBaseUrl}/webhook/scan/dast",
                "PORTS_SCAN" => $"{n8nBaseUrl}/webhook/scan/ports",
                "SECRETS_SCAN" => $"{n8nBaseUrl}/webhook/scan/secrets",
                _ => throw new ArgumentException($"Tipo de escaneo no soportado: {scanType}")
            };
        }
    }
}