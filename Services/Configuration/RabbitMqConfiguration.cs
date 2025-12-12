using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.SecurityGate.Service.Configuration
{
    public class RabbitMqConfiguration
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        
        // Exchanges
        public string ScanRequestExchange { get; set; } = "sentinel.scan.requests";
        public string ScanResultExchange { get; set; } = "sentinel.scan.results";
        
        // Queues
        public string ScanRequestQueue { get; set; } = "sentinel.scan.requests.queue";
        public string ScanResultQueue { get; set; } = "sentinel.scan.results.queue";
        
        // Routing Keys
        public string SastRoutingKey { get; set; } = "scan.sast";
        public string DastRoutingKey { get; set; } = "scan.dast";
        public string PortScanRoutingKey { get; set; } = "scan.ports";
        public string SecretScanRoutingKey { get; set; } = "scan.secrets";
        
        public string GetRoutingKeyForScanType(string scanType)
        {
            return scanType.ToUpperInvariant() switch
            {
                "FULL_SAST" or "SAST" => SastRoutingKey,
                "DAST_BASIC" or "DAST" => DastRoutingKey,
                "PORTS_SCAN" => PortScanRoutingKey,
                "SECRETS_SCAN" => SecretScanRoutingKey,
                _ => $"scan.{scanType.ToLower()}"
            };
        }
    }
}