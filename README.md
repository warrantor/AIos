# AIos

A high-performance, self-hostable orchestration platform for LLM-powered conversational workflows. Built with .NET 10 for speed and reliability, with a plugin ecosystem that supports Python, TypeScript, and C#.

## What is AIos?

AIos sits between your users and LLM infrastructure, handling:

- **Conversation Management** — Create, resume, and maintain multi-turn conversations with full history persistence
- **Real-time Streaming** — Token-by-token LLM response streaming via WebSocket
- **Plugin Extensibility** — Add custom tools, data sources, and logic without touching the core
- **Multi-Client Support** — Access via REST API, CLI, Web UI, or custom SDKs
- **Enterprise-Grade Persistence** — PostgreSQL-backed state management for conversations, messages, and metadata

## Quick Start

### Requirements

- Docker & Docker Compose (easiest path)
- Or: .NET 10 SDK + PostgreSQL 15+

### Run with Docker Compose

```bash
docker-compose up
```

This starts:
- **AIos Gateway** (API + WebSocket server) on `http://localhost:5000`
- **PostgreSQL** database
- *(Optional) Next.js Web UI* — see `UI/Web` for details

### Run Locally (.NET)

1. **Setup database:**
   ```bash
   # Ensure PostgreSQL is running on localhost:5432
   # Default user: postgres, password: postgres
   dotnet ef database update --project AIos.Core
   ```

2. **Start the gateway:**
   ```bash
   dotnet run --project AIos.Gateway
   ```

3. **Test the API:**
   ```bash
   curl http://localhost:5000/health
   ```

## Architecture

```
┌─────────────────────────────────────────┐
│         Web UI / CLI / SDK Clients      │
└────────────┬────────────────────────────┘
             │ REST / WebSocket
             ▼
┌─────────────────────────────────────────┐
│      AIos.Gateway (FastEndpoints)       │
│  ├─ REST API                            │
│  ├─ WebSocket Streaming                 │
│  └─ Request Orchestration               │
└────────────┬────────────────────────────┘
             │ Business Logic
             ▼
┌─────────────────────────────────────────┐
│      AIos.Core (Domain & Persistence)   │
│  ├─ Conversation Lifecycle              │
│  ├─ Message History                     │
│  ├─ Plugin Coordination                 │
│  ├─ LLM Integration                     │
│  └─ Automation Data                     │
└────────────┬────────────────────────────┘
             │ ORM (EF Core)
             ▼
┌─────────────────────────────────────────┐
│         PostgreSQL Database             │
└─────────────────────────────────────────┘

Side-channel: Plugins/Addons (REST/gRPC)
```

### Project Structure

```
AIos/
├── Core/
│   └── AIos.Core.csproj              # Domain, persistence, orchestration
├── Gateway/
│   ├── AIos.Gateway.csproj           # REST + WebSocket API
│   └── Dockerfile
├── Addons/SDK/
│   ├── Dotnet/src/AIos.Sdk/          # .NET client SDK
│   ├── Python/src/                   # Python SDK (coming)
│   └── TypeScript/src/               # TypeScript SDK (coming)
├── UI/
│   ├── CLI/src/AIos.Cli/             # Command-line interface
│   └── Web/src/                      # Next.js web interface
├── compose.yaml                      # Docker Compose config
└── README.md                         # This file
```

## API Overview

### Create a Conversation

```bash
POST /api/conversations
Content-Type: application/json

{
  "title": "My First Chat"
}

# Response
{
  "id": "conv-abc123",
  "title": "My First Chat",
  "createdAt": "2025-03-17T12:00:00Z"
}
```

### Send a Message

```bash
POST /api/conversations/{conversationId}/messages
Content-Type: application/json

{
  "content": "What is 2+2?"
}
```

### Stream LLM Response (WebSocket)

```javascript
const ws = new WebSocket("ws://localhost:5000/api/conversations/{conversationId}/stream");

ws.onmessage = (event) => {
  const chunk = JSON.parse(event.data);
  console.log(chunk.token); // Print token as it arrives
};

ws.send(JSON.stringify({
  content: "Explain quantum computing",
  modelId: "gpt-4"
}));
```

### Retrieve Conversation History

```bash
GET /api/conversations/{conversationId}/messages

# Response
{
  "messages": [
    {
      "id": "msg-001",
      "role": "user",
      "content": "Hello!",
      "createdAt": "2025-03-17T12:00:00Z"
    },
    {
      "id": "msg-002",
      "role": "assistant",
      "content": "Hi! How can I help?",
      "createdAt": "2025-03-17T12:00:05Z"
    }
  ]
}
```

## Building Plugins

Plugins extend AIos without modifying the core. They communicate via REST or gRPC.

### Example: Python Plugin

```python
# plugins/sentiment_analyzer.py
from aios_sdk import Plugin, PluginContext

class SentimentPlugin(Plugin):
    def on_message_received(self, context: PluginContext):
        """Analyze sentiment before LLM processes the message."""
        sentiment = analyze_text(context.message.content)
        context.metadata["sentiment"] = sentiment
        return context

# Register with AIos
plugin = SentimentPlugin(
    name="sentiment-analyzer",
    version="1.0.0",
    webhook_url="http://localhost:9000/hook"
)
plugin.register_with_gateway("http://localhost:5000")
```

### Example: TypeScript Plugin

```typescript
// plugins/webhook-logger.ts
import { Plugin, PluginContext } from "@aios/sdk";

export const webhookLogger: Plugin = {
  name: "webhook-logger",
  version: "1.0.0",
  
  async onMessageReceived(context: PluginContext) {
    await fetch("https://my-service.com/log", {
      method: "POST",
      body: JSON.stringify(context)
    });
    return context;
  }
};
```

## Configuration

Environment variables (`.env`):

```env
# Database
DATABASE_URL=postgres://postgres:postgres@localhost:5432/aios

# LLM Provider
LLM_PROVIDER=openai
OPENAI_API_KEY=sk-...

# Server
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Development

# Plugins
PLUGIN_REGISTRY_URL=http://plugin-registry:8080
```

## CLI Usage

```bash
# Start a new conversation
aios chat start "My Project Assistant"

# Send a message
aios chat send "What are the next steps?"

# View history
aios chat history

# List conversations
aios chat list
```

## Performance

AIos is designed for speed:

- **.NET JIT/AOT compilation** for near-native execution
- **Minimal allocations** via value types and pooling
- **Streaming-first** architecture to reduce latency
- **Efficient concurrency** using async/await and channels

Benchmarks (coming in v1.0):
- Sub-100ms API response times
- Sustained 10k concurrent WebSocket connections
- Zero-copy streaming pipelines

## Development

### Prerequisites

- .NET 10 SDK
- PostgreSQL 18
- Docker/Podman (recommended)

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
# Build the Docker image
docker build -f AIos.Gateway/Dockerfile -t aios-gateway:latest .

# Push to registry
docker push your-registry/aios-gateway:latest
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName --project AIos.Core

# Apply migrations
dotnet ef database update --project AIos.Core
```

## Roadmap

- [x] Core conversation & message persistence
- [x] REST API
- [x] WebSocket streaming
- [ ] Plugin SDK (Python, TypeScript)
- [ ] Next.js Web UI
- [ ] Authentication & multi-tenant support
- [ ] Audit logging
- [ ] Observability (OpenTelemetry)
- [ ] Vector embeddings & RAG
- [ ] Marketplace for plugins

## Contributing

We welcome contributions! Please see `CONTRIBUTING.md` for guidelines. (when it gets added)

### To get started:

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes
4. Write tests
5. Submit a PR

## Licensing

AIos is released under the **MIT License**. See `LICENSE` for details.
