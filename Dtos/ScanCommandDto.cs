namespace Sentinel.SecurityGate.Service.Dtos;

/// <summary>
/// DTO para comandos de escaneo con todos los campos requeridos para SAST y DAST
/// </summary>
public class ScanCommandDto
{
    public Guid ScanId { get; set; }
    
    /// <summary>
    /// Tipo de escaneo: SAST, DAST, PORTS_SCAN, SECRETS_SCAN, etc.
    /// </summary>
    public required string ScanType { get; set; }
    
    // ========== CAMPOS PARA SAST (Semgrep) ==========
    
    /// <summary>
    /// URL del repositorio Git (para SAST)
    /// Ejemplo: https://github.com/usuario/repositorio
    /// </summary>
    public string? RepositoryUrl { get; set; }
    
    /// <summary>
    /// Rama específica a escanear (para SAST)
    /// Ejemplo: main, dev, feature/nueva-funcionalidad
    /// </summary>
    public string? Branch { get; set; }
    
    /// <summary>
    /// Commit ID específico (opcional, para SAST)
    /// </summary>
    public string? CommitId { get; set; }
    
    /// <summary>
    /// Token de acceso al repositorio Git
    /// </summary>
    public string? ClientGitToken { get; set; }
    
    /// <summary>
    /// Ruta local del código (si ya está clonado)
    /// </summary>
    public string? LocalCodePath { get; set; }
    
    // ========== CAMPOS PARA DAST (OWASP ZAP) ==========
    
    /// <summary>
    /// URL base de la aplicación en ejecución (para DAST)
    /// Ejemplo: https://staging.mi-app.com
    /// </summary>
    public string? TargetUrl { get; set; }
    
    /// <summary>
    /// Credenciales de autenticación para DAST
    /// </summary>
    public DastAuthenticationDto? Authentication { get; set; }
    
    /// <summary>
    /// Lista de endpoints específicos a escanear (alcance del DAST)
    /// </summary>
    public List<string>? ScanScope { get; set; }
    
    /// <summary>
    /// Configuración del spider/crawler para DAST
    /// </summary>
    public DastSpiderConfigDto? SpiderConfig { get; set; }
    
    // ========== CAMPOS COMUNES ==========
    
    /// <summary>
    /// ID del cliente que solicita el escaneo
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Configuración adicional específica del tipo de escaneo
    /// </summary>
    public Dictionary<string, string>? AdditionalConfig { get; set; }
    
    /// <summary>
    /// Timeout máximo para el escaneo (en minutos)
    /// </summary>
    public int? TimeoutMinutes { get; set; }
}

/// <summary>
/// Configuración de autenticación para DAST
/// </summary>
public class DastAuthenticationDto
{
    /// <summary>
    /// Tipo de autenticación: Basic, FormBased, Token, OAuth
    /// </summary>
    public string? AuthType { get; set; }
    
    /// <summary>
    /// Usuario para autenticación
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Contraseña para autenticación
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// Token de sesión o Bearer token
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// URL del endpoint de login (para autenticación basada en formulario)
    /// </summary>
    public string? LoginUrl { get; set; }
    
    /// <summary>
    /// Nombres de los campos del formulario de login
    /// </summary>
    public Dictionary<string, string>? LoginFormFields { get; set; }
    
    /// <summary>
    /// Cookies de sesión (para mantener sesión activa)
    /// </summary>
    public Dictionary<string, string>? SessionCookies { get; set; }
}

/// <summary>
/// Configuración del spider/crawler para DAST
/// </summary>
public class DastSpiderConfigDto
{
    /// <summary>
    /// Habilitar spider automático
    /// </summary>
    public bool EnableSpider { get; set; } = true;
    
    /// <summary>
    /// Profundidad máxima del spider
    /// </summary>
    public int MaxDepth { get; set; } = 5;
    
    /// <summary>
    /// URLs semilla para iniciar el crawling
    /// </summary>
    public List<string>? SeedUrls { get; set; }
    
    /// <summary>
    /// Patrones de URL a excluir del escaneo
    /// </summary>
    public List<string>? ExcludePatterns { get; set; }
    
    /// <summary>
    /// Tiempo máximo de spider en minutos
    /// </summary>
    public int? SpiderTimeoutMinutes { get; set; }
}