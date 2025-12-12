using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

namespace Sentinel.SecurityGate.Service.Models
{
    /// <summary>
    /// Representa un hallazgo de seguridad unificado (SAST o DAST)
    /// </summary>
    public class Finding
    {
        /// <summary>
        /// ID único del hallazgo
        /// </summary>
        public string FindingId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Tipo de escaneo que detectó este hallazgo (SAST, DAST)
        /// </summary>
        public string ScanType { get; set; }
        
        /// <summary>
        /// Identificador de la regla que detectó la vulnerabilidad
        /// Ejemplo SAST: "java.lang.security.hardcoded-secret"
        /// Ejemplo DAST: "10021" (ID de alerta de ZAP)
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Nombre descriptivo de la regla
        /// </summary>
        public string RuleName { get; set; }
        
        /// <summary>
        /// Descripción de la vulnerabilidad encontrada
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Severidad: Critical, High, Medium, Low, Info
        /// </summary>
        public string Severity { get; set; }
        
        /// <summary>
        /// Puntuación de riesgo (0-10, compatible con CVSS)
        /// </summary>
        public double? RiskScore { get; set; }
        
        // ========== CAMPOS ESPECÍFICOS DE SAST ==========
        
        /// <summary>
        /// Ruta del archivo donde se encontró la vulnerabilidad (SAST)
        /// Ejemplo: "src/main/java/com/example/UserController.java"
        /// </summary>
        public string? FilePath { get; set; }
        
        /// <summary>
        /// Número de línea donde inicia la vulnerabilidad (SAST)
        /// </summary>
        public int? LineNumber { get; set; }
        
        /// <summary>
        /// Número de columna donde inicia la vulnerabilidad (SAST)
        /// </summary>
        public int? ColumnNumber { get; set; }
        
        /// <summary>
        /// Línea final de la vulnerabilidad (SAST)
        /// </summary>
        public int? EndLineNumber { get; set; }
        
        /// <summary>
        /// Fragmento de código relevante (SAST)
        /// </summary>
        public string? CodeSnippet { get; set; }
        
        /// <summary>
        /// Función o método donde se encontró la vulnerabilidad (SAST)
        /// </summary>
        public string? FunctionName { get; set; }
        
        // ========== CAMPOS ESPECÍFICOS DE DAST ==========
        
        /// <summary>
        /// URL o endpoint afectado (DAST)
        /// Ejemplo: "https://staging.mi-app.com/api/users"
        /// </summary>
        public string? AffectedUrl { get; set; }
        
        /// <summary>
        /// Método HTTP utilizado (DAST)
        /// Ejemplo: GET, POST, PUT, DELETE
        /// </summary>
        public string? HttpMethod { get; set; }
        
        /// <summary>
        /// Parámetros de la solicitud que desencadenaron la vulnerabilidad (DAST)
        /// </summary>
        public Dictionary<string, string>? RequestParameters { get; set; }
        
        /// <summary>
        /// Evidencia de la vulnerabilidad - fragmento de la respuesta HTTP (DAST)
        /// </summary>
        public string? Evidence { get; set; }
        
        /// <summary>
        /// Cuerpo de la solicitud HTTP completa (DAST)
        /// </summary>
        public string? RequestBody { get; set; }
        
        /// <summary>
        /// Cuerpo de la respuesta HTTP (DAST)
        /// </summary>
        public string? ResponseBody { get; set; }
        
        /// <summary>
        /// Código de estado HTTP de la respuesta (DAST)
        /// </summary>
        public int? HttpStatusCode { get; set; }
        
        /// <summary>
        /// Headers de la solicitud (DAST)
        /// </summary>
        public Dictionary<string, string>? RequestHeaders { get; set; }
        
        // ========== CAMPOS COMUNES ==========
        
        /// <summary>
        /// Tipo de vulnerabilidad según clasificación estándar
        /// Ejemplo: "SQL Injection", "XSS", "Hardcoded Secret"
        /// </summary>
        public string VulnerabilityType { get; set; }
        
        /// <summary>
        /// Mapeo a OWASP Top 10
        /// Ejemplo: "A03:2021 – Injection"
        /// </summary>
        public string? OwaspCategory { get; set; }
        
        /// <summary>
        /// Mapeo a CWE (Common Weakness Enumeration)
        /// Ejemplo: "CWE-89: SQL Injection"
        /// </summary>
        public string? CweId { get; set; }
        
        /// <summary>
        /// Sugerencias de corrección/mitigación
        /// </summary>
        public string? RemediationSuggestion { get; set; }
        
        /// <summary>
        /// Referencias externas (CVE, artículos, documentación)
        /// </summary>
        public List<string>? References { get; set; }
        
        /// <summary>
        /// Nivel de confianza en el hallazgo (High, Medium, Low)
        /// </summary>
        public string? Confidence { get; set; }
        
        /// <summary>
        /// Indica si es un falso positivo
        /// </summary>
        public bool IsFalsePositive { get; set; }
        
        /// <summary>
        /// Timestamp de cuando se detectó
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Metadata adicional específica de la herramienta
        /// </summary>
        public Dictionary<string, object>? ToolSpecificData { get; set; }
    }
}