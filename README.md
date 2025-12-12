# 🛡️ Sentinel SecurityGate Service

**Sentinel SecurityGate Service** is the central security orchestrator in the Sentinel architecture. It acts as the gateway for security scan requests (SAST, DAST, etc.), coordinating workflows in n8n and managing asynchronous communication through RabbitMQ.

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [API Endpoints](#-api-endpoints)
- [Scan Types](#-scan-types)
- [Data Structures](#-data-structures)
- [n8n Integration](#-n8n-integration)
- [RabbitMQ](#-rabbitmq)
- [Troubleshooting](#-troubleshooting)

---

## ✨ Features

- ✅ **Scan Orchestration**: Coordinates SAST (Semgrep) and DAST (OWASP ZAP) scans
- ✅ **Event-Driven Architecture**: Asynchronous communication via RabbitMQ
- ✅ **n8n Triggers**: Initiates automated workflows via HTTP webhooks
- ✅ **Granular Scans**: Allows selection of specific scan types
- ✅ **Normalized Results**: Unified structure for security findings
- ✅ **Quality Gate**: Automatic pass/fail evaluation
- ✅ **Standards Mapping**: OWASP Top 10 and CWE
- ✅ **Automated Actions**: Ticket creation (Jira) and notifications (Slack, Teams)
- ✅ **Background Listeners**: Continuous result processing from RabbitMQ

---

## 🏗️ Architecture

```
┌─────────────┐      ┌──────────────────┐      ┌──────────┐
│  Java BFF   │─────▶│ SecurityGate     │─────▶│   n8n    │
│             │      │ Service (.NET)   │      │ Workflow │
└─────────────┘      └──────────────────┘      └──────────┘
                              │                       │
                              ▼                       ▼
                     ┌─────────────────┐    ┌─────────────┐
                     │    RabbitMQ     │    │  Semgrep /  │
                     │  Message Broker │    │  OWASP ZAP  │
                     └─────────────────┘    └─────────────┘
                              │                       │
                              ▼                       ▼
                     ┌─────────────────┐    ┌─────────────┐
                     │  Background     │    │    Scan     │
                     │  Listener       │◀───│   Results   │
                     └─────────────────┘    └─────────────┘
```

### Workflow

1. **Java BFF** → `POST /api/scan/request` → **SecurityGate**
2. **SecurityGate** → Trigger webhook → **n8n**
3. **SecurityGate** → Publish message → **RabbitMQ** (`sentinel.scan.requests`)
4. **n8n** → Execute scan (Semgrep/ZAP)
5. **n8n** → Send result → `POST /api/scan/webhook/result`
6. **SecurityGate** → Publish result → **RabbitMQ** (`sentinel.scan.results`)
7. **ScanResultListener** → Consume result → Notify **Java BFF**

---

## 📦 Prerequisites

- **.NET 8.0 SDK** or higher
- **RabbitMQ** (localhost:5672 or remote server)
- **n8n** (for workflow orchestration)
- **(Optional)** Docker for containerized deployment

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-company/sentinel-securitygate-service.git
cd sentinel-securitygate-service
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Project

```bash
dotnet build
```

### 4. Run the Service

```bash
dotnet run
```

The service will be available at:
- **HTTP**: `http://localhost:5275`
- **HTTPS**: `https://localhost:7188`
- **Swagger UI**: `http://localhost:5275/swagger`

---

## ⚙️ Configuration

### `appsettings.json`

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ScanRequestExchange": "sentinel.scan.requests",
    "ScanResultExchange": "sentinel.scan.results",
    "ScanRequestQueue": "sentinel.scan.requests.queue",
    "ScanResultQueue": "sentinel.scan.results.queue"
  },
  "N8n": {
    "BaseUrl": "http://localhost:5678"
  },
  "JavaBFF": {
    "WebhookUrl": "http://localhost:8080/api/scans/webhook",
    "Origin": "http://localhost:8080"
  }
}
```

### Environment Variables (Production)

```bash
export RabbitMQ__HostName="rabbitmq.prod.com"
export RabbitMQ__UserName="sentinel"
export RabbitMQ__Password="super-secret-password"
export N8n__BaseUrl="https://n8n.prod.com"
```

---

## 📖 Usage

### Request a SAST Scan

```bash
curl -X POST http://localhost:5275/api/scan/request \
  -H "Content-Type: application/json" \
  -d '{
    "scanType": "SAST",
    "clientId": "client-12345",
    "repositoryUrl": "https://github.com/my-company/my-app",
    "branch": "main",
    "clientGitToken": "ghp_xxxxxxxxxxxxx"
  }'
```

**Response (HTTP 202 Accepted):**

```json
{
  "scanId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "status": "ACCEPTED",
  "requestedService": "SAST",
  "acceptanceTimestampUtc": "2025-12-10T15:30:00Z",
  "completionMethod": "RABBITMQ_EVENT"
}
```

### Request a DAST Scan

```bash
curl -X POST http://localhost:5275/api/scan/request \
  -H "Content-Type: application/json" \
  -d '{
    "scanType": "DAST",
    "clientId": "client-12345",
    "targetUrl": "https://staging.my-app.com",
    "authentication": {
      "authType": "FormBased",
      "username": "test-user",
      "password": "test-password",
      "loginUrl": "https://staging.my-app.com/login"
    }
  }'
```

### Check Scan Status

```bash
curl -X GET http://localhost:5275/api/scan/{scanId}/status
```

---

## 🔌 API Endpoints

### Health Check

```http
GET /api/health
```

Checks the service status.

**Response:**

```json
{
  "status": "Healthy",
  "service": "Sentinel.SecurityGate.Service",
  "timestamp": "2025-12-10T15:30:00Z"
}
```

---

### Request Scan

```http
POST /api/scan/request
Content-Type: application/json
```

**Body:** See [Data Structures](#-data-structures) section

**Response:** `202 Accepted` with `ScanAcceptanceDto`

---

### Get Scan Status

```http
GET /api/scan/{scanId}/status
```

**Response:**

```json
{
  "scanId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "status": "IN_PROGRESS",
  "scanType": "SAST",
  "target": "https://github.com/example/repo",
  "startedAt": "2025-12-10T15:30:05Z",
  "progressPercentage": 45
}
```

---

### Webhook for Results (n8n → SecurityGate)

```http
POST /api/scan/webhook/result
Content-Type: application/json
```

**Body:** Complete `ScanResult` (see examples in code)

---

## 🎯 Scan Types

| Type | Tool | Description |
|------|------|-------------|
| `SAST` / `FULL_SAST` | Semgrep | Static analysis of source code |
| `DAST` / `DAST_BASIC` | OWASP ZAP | Dynamic analysis of running application |
| `SECRETS_SCAN` | TruffleHog / Gitleaks | Search for hardcoded secrets |
| `PORTS_SCAN` | Nmap | Open port scanning |

---

## 📊 Data Structures

### ScanCommandDto (Input)

#### SAST Request Example

```json
{
  "scanType": "SAST",
  "clientId": "client-12345",
  "repositoryUrl": "https://github.com/user/repo",
  "branch": "main",
  "clientGitToken": "ghp_token",
  "timeoutMinutes": 30
}
```

#### DAST Request Example

```json
{
  "scanType": "DAST",
  "clientId": "client-12345",
  "targetUrl": "https://staging.my-app.com",
  "authentication": {
    "authType": "FormBased",
    "username": "test-user",
    "password": "test-password",
    "loginUrl": "https://staging.my-app.com/login",
    "loginFormFields": {
      "usernameField": "email",
      "passwordField": "password"
    }
  },
  "scanScope": [
    "https://staging.my-app.com/api/users",
    "https://staging.my-app.com/api/products"
  ],
  "spiderConfig": {
    "enableSpider": true,
    "maxDepth": 5,
    "seedUrls": [
      "https://staging.my-app.com/dashboard"
    ],
    "excludePatterns": [".*/logout", ".*/admin/.*"],
    "spiderTimeoutMinutes": 10
  },
  "timeoutMinutes": 60
}
```

### ScanResult (Output)

```json
{
  "scanId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "scanType": "SAST",
  "qualityGatePassed": false,
  "summary": {
    "critical": 2,
    "high": 5,
    "medium": 12,
    "low": 8,
    "info": 3,
    "total": 30
  },
  "findings": [
    {
      "ruleId": "java.lang.security.sql-injection",
      "severity": "High",
      "filePath": "src/UserDao.java",
      "lineNumber": 45,
      "codeSnippet": "String query = \"SELECT * FROM users WHERE id=\" + userId;",
      "owaspCategory": "A03:2021 – Injection",
      "cweId": "CWE-89",
      "remediationSuggestion": "Use parameterized queries"
    }
  ],
  "metrics": {
    "executionTimeSeconds": 315.5,
    "filesAnalyzed": 234,
    "linesOfCode": 45678
  },
  "actions": {
    "ticketIds": ["JIRA-1234"],
    "notificationIds": ["slack-msg-abc123"],
    "ticketUrls": ["https://jira.company.com/browse/JIRA-1234"]
  }
}
```

### Finding (Unified Finding)

Each finding includes:

**Common Fields:**
- `ruleId`, `severity`, `description`
- `owaspCategory`, `cweId`
- `remediationSuggestion`

**SAST-Specific Fields:**
- `filePath`, `lineNumber`, `columnNumber`
- `codeSnippet`, `functionName`

**DAST-Specific Fields:**
- `affectedUrl`, `httpMethod`
- `requestParameters`, `evidence`
- `httpStatusCode`, `responseBody`

---

## 🔄 n8n Integration

### Configure Webhooks in n8n

Create workflows that listen on:

- **SAST**: `http://n8n:5678/webhook/scan/sast`
- **DAST**: `http://n8n:5678/webhook/scan/dast`
- **Secrets**: `http://n8n:5678/webhook/scan/secrets`
- **Ports**: `http://n8n:5678/webhook/scan/ports`

### Payload n8n Receives

#### SAST Payload

```json
{
  "scanId": "uuid",
  "scanType": "SAST",
  "clientId": "client-12345",
  "timestamp": "2025-12-10T15:30:00Z",
  "repository": {
    "url": "https://github.com/user/repo",
    "branch": "main",
    "commitId": null,
    "token": "ghp_token"
  },
  "config": {
    "localPath": null,
    "timeoutMinutes": 30,
    "additionalConfig": {}
  }
}
```

#### DAST Payload

```json
{
  "scanId": "uuid",
  "scanType": "DAST",
  "clientId": "client-12345",
  "timestamp": "2025-12-10T16:00:00Z",
  "target": {
    "url": "https://staging.my-app.com",
    "scope": []
  },
  "authentication": {
    "authType": "FormBased",
    "username": "test-user",
    "password": "test-password",
    "loginUrl": "https://staging.my-app.com/login"
  },
  "spider": {
    "enabled": true,
    "maxDepth": 5,
    "seedUrls": [],
    "excludePatterns": [],
    "timeoutMinutes": 10
  },
  "config": {
    "timeoutMinutes": 60
  }
}
```

### n8n Sends Result to:

```http
POST http://securitygate:5275/api/scan/webhook/result
```

---

## 🐰 RabbitMQ

### Exchanges

- **`sentinel.scan.requests`** (Topic): Scan requests
- **`sentinel.scan.results`** (Fanout): Scan results

### Queues

- **`sentinel.scan.requests.queue`**: Request queue
- **`sentinel.scan.results.queue`**: Results queue

### Routing Keys

- `scan.sast` → SAST scans
- `scan.dast` → DAST scans
- `scan.ports` → Port scans
- `scan.secrets` → Secret searches

### Consumer Example (Python Worker)

```python
import pika
import json

connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()

def callback(ch, method, properties, body):
    scan_request = json.loads(body)
    print(f"Processing scan {scan_request['scanId']}")
    # Process scan...

channel.basic_consume(
    queue='sentinel.scan.requests.queue',
    on_message_callback=callback,
    auto_ack=True
)

channel.start_consuming()
```

---

## 🐛 Troubleshooting

### Error: "Cannot connect to RabbitMQ"

**Solution:**
1. Verify RabbitMQ is running: `docker ps | grep rabbitmq`
2. Test connection: `telnet localhost 5672`
3. Check credentials in `appsettings.json`

### Error: "n8n webhook timeout"

**Solution:**
1. Increase `TimeoutMinutes` in `ScanCommandDto`
2. Verify n8n is running: `curl http://localhost:5678/healthz`
3. Check n8n logs

### Results not reaching Java BFF

**Solution:**
1. Verify `JavaBFF:WebhookUrl` configuration in `appsettings.json`
2. Check `ScanResultListener` logs
3. Verify Java BFF has an active POST endpoint

### "Scan stays in PENDING indefinitely"

**Solution:**
1. Check n8n logs for workflow errors
2. Verify n8n workflow sends result to SecurityGate webhook
3. Confirm RabbitMQ is processing messages

---

## 📝 Logs

Logs are generated in:
- **Console**: During development
- **Files**: Configure in `appsettings.json` with Serilog

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Sentinel.SecurityGate.Service": "Debug"
  }
}
```

---

## 🚢 Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5275

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Sentinel.SecurityGate.Service.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentinel.SecurityGate.Service.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: sentinel
      RABBITMQ_DEFAULT_PASS: sentinel123

  securitygate:
    build: .
    ports:
      - "5275:5275"
    depends_on:
      - rabbitmq
    environment:
      RabbitMQ__HostName: rabbitmq
      RabbitMQ__UserName: sentinel
      RabbitMQ__Password: sentinel123
      N8n__BaseUrl: http://n8n:5678
```

Run:

```bash
docker-compose up -d
```

---

## 🤝 Contributing

1. Fork the project
2. Create your feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License. See `LICENSE` file for details.

---

## 👥 Team

- **Lead Developer**: [Your Name]
- **Software Architect**: [Name]
- **DevOps**: [Name]

---

## 📞 Support

- **Email**: support@sentinel.com
- **Slack**: #sentinel-security-gate
- **Jira**: [SENTINEL Project](https://jira.company.com/projects/SENTINEL)

---

## 🔗 Useful Links

- [n8n Documentation](https://docs.n8n.io/)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Semgrep Rules](https://semgrep.dev/explore)
- [OWASP ZAP User Guide](https://www.zaproxy.org/docs/)

---

**Thank you for using Sentinel SecurityGate Service!** 🛡️🚀
