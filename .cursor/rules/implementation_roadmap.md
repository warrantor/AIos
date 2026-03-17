---
apply: always
---

# AIos Implementation Roadmap

## Current State
- ✅ Basic conversation management implemented
- ✅ Message history persistence (PostgreSQL)
- ✅ Hardcoded OpenAI model integration
- ✅ REST API endpoints for conversation lifecycle
- ⚠️ WebSocket/streaming partially implemented
- ❌ Plugin/addon system not yet implemented
- ❌ Multi-model provider support incomplete
- ❌ Full token streaming pipeline not finalized

---

## Phase 1: Core Platform Stabilization

### 1.1 Model Provider Abstraction
- [ ] Create `ILlmProvider` interface in Core
  - Methods: `CompleteAsync()`, `StreamCompleteAsync()`
  - Support request/response models that work with any provider
- [ ] Implement `OpenAiProvider` using the interface
- [ ] Implement `AnthropicProvider` (Claude API)
- [ ] Add provider configuration/factory pattern
- [ ] Store provider selection in conversation metadata
- [ ] **Test**: Verify switching providers doesn't break conversation flow

### 1.2 Token Streaming Pipeline
- [ ] Refine WebSocket streaming implementation (SignalR)
- [ ] Ensure tokens stream from provider → Gateway → WebSocket → UI
- [ ] Implement proper stream cancellation/interruption handling
- [ ] Add heartbeat/keep-alive for long streams
- [ ] **Test**: Stream 1000+ token response, verify no loss

### 1.3 Message Metadata & Streaming State
- [ ] Extend `Message` domain model with:
  - `StreamingStatus` (pending, streaming, completed, failed)
  - `TokenCount` (input, output)
  - `ModelUsed` reference
  - `ProviderMetadata` (JSON store for provider-specific data)
- [ ] Update database schema and migrations
- [ ] **Test**: Verify metadata persists correctly

### 1.4 Error Handling & Resilience
- [ ] Implement retry logic for provider calls (exponential backoff)
- [ ] Add timeout configuration per provider
- [ ] Graceful degradation when a provider is unavailable
- [ ] Proper error messages returned to client (not internal stack traces)
- [ ] **Test**: Simulate provider failures, verify recovery

---

## Phase 2: Plugin/Addon System Foundation

### 2.1 Plugin Registry & Discovery
- [ ] Create `Plugin` domain entity with:
  - Name, version, description
  - Supported hooks (pre-message, post-message, tool-call, etc.)
  - Configuration schema (JSON)
  - Status (enabled/disabled)
- [ ] Create `PluginRegistry` service in Core
  - In-memory registry initially
  - Later: persistent storage in PostgreSQL
- [ ] Implement plugin registration endpoint in Gateway
- [ ] **Test**: Register/list plugins via API

### 2.2 Plugin Contract Definition
- [ ] Define standardized webhook/event model:
  ```
  {
    "event": "pre-message" | "post-message" | "tool-call",
    "conversation_id": "...",
    "message": {...},
    "metadata": {...}
  }
  ```
- [ ] Define plugin response model
- [ ] Document contract in `/docs/plugin-api.md`
- [ ] **Test**: Manual curl test of plugin endpoint

### 2.3 Plugin Invocation Framework
- [ ] Create `IPluginExecutor` interface
  - `ExecuteAsync(pluginId, event, payload)`
  - Timeout and error handling built-in
- [ ] Implement HTTP-based executor (plugins as external services)
- [ ] Implement retry logic with exponential backoff
- [ ] Add plugin execution logging
- [ ] **Test**: Invoke plugins in pre/post-message hooks

### 2.4 SDK Scaffolding
- [ ] **.NET SDK** (`AIos.Sdk`):
  - Client class for API access
  - Plugin helper for building plugins
  - Type-safe contract models
  - Example plugin in `Addons/SDK/Dotnet/examples/`
  
- [ ] **Python SDK** (`Addons/SDK/Python`):
  - Project structure (src/, tests/, setup.py)
  - Client library (async support)
  - Plugin decorator/helper
  - Example plugin
  
- [ ] **TypeScript SDK** (`Addons/SDK/Typescript`):
  - Project structure (src/, tests/, package.json)
  - Client library (async/await)
  - Plugin helper
  - Example plugin

- [ ] **Test**: Each SDK can call API and register a plugin

---

## Phase 3: Advanced Model Features

### 3.1 Tool/Function Calling
- [ ] Add `Tool` domain entity:
  - Name, description, input schema (JSON Schema)
  - Output format
  - Associated with plugins or built-in
- [ ] Extend message model to support tool calls and results
- [ ] Implement tool call detection in LLM orchestration
- [ ] Chain tool results back into conversation
- [ ] **Test**: Multi-turn conversation with tool calls

### 3.2 Conversation Context Management
- [ ] Implement sliding window for token context
  - Compress older messages if context window exceeded
  - Configurable context size per model/provider
- [ ] Add system prompt management (per conversation or global)
- [ ] **Test**: 100+ message conversation stays within context

### 3.3 Message Roles & Types
- [ ] Extend `Message` to support:
  - Roles: user, assistant, system, tool
  - Types: text, tool-call, tool-result, image, document
- [ ] Update serialization/deserialization
- [ ] **Test**: Mixed message types in single conversation

---

## Phase 4: User & Access Management

### 4.1 User Model & Authentication
- [ ] Create `User` domain entity
  - Id, email, created_at, settings
- [ ] Implement JWT-based auth (or OAuth2)
- [ ] Create `/auth/login` and `/auth/register` endpoints
- [ ] Add auth middleware to Gateway
- [ ] Associate conversations with users
- [ ] **Test**: Create user, log in, verify token

### 4.2 API Keys for Programmatic Access
- [ ] Create `ApiKey` entity (hashed storage)
- [ ] Implement key generation/revocation endpoints
- [ ] Add API key authentication as alternative to JWT
- [ ] Rate limiting per API key
- [ ] **Test**: Access API using key instead of JWT

### 4.3 User Settings & Preferences
- [ ] Create `UserSettings` entity
  - Default model/provider preference
  - Default system prompt
  - Theme (dark/light)
- [ ] Endpoints to get/update settings
- [ ] **Test**: User settings persist across sessions

---

## Phase 5: Automation & Workflow

### 5.1 Automation Rules Engine
- [ ] Create `Automation` domain entity:
  - Trigger (e.g., "message contains keyword")
  - Action (e.g., "invoke plugin X", "switch model")
  - Condition logic (if/then)
- [ ] `AutomationEngine` service to evaluate and execute
- [ ] Store automations in PostgreSQL
- [ ] **Test**: Trigger automation on incoming message

### 5.2 Scheduled Tasks
- [ ] Implement background job scheduler (Hangfire or similar)
- [ ] Support scheduled conversations (e.g., daily sync)
- [ ] **Test**: Schedule a task, verify execution

---

## Phase 6: CLI & Web UI

### 6.1 CLI Enhancements
- [ ] Implement commands:
  - `aios chat [conversation-id]` — resume conversation
  - `aios new` — start new conversation
  - `aios list` — list conversations
  - `aios plugin list` — list registered plugins
  - `aios config` — manage settings
- [ ] Interactive REPL-style chat mode
- [ ] **Test**: Full CLI conversation flow

### 6.2 Web UI (Next.js)
- [ ] Set up Next.js project structure
- [ ] Create auth page (login/register)
- [ ] Create conversation list page
- [ ] Create chat interface (real-time streaming)
- [ ] Create plugin management UI
- [ ] Create user settings page
- [ ] **Test**: End-to-end web flow

---

## Phase 7: Observability & DevOps

### 7.1 Logging & Monitoring
- [ ] Structured logging (Serilog) in Core/Gateway
- [ ] Log levels: Info, Warn, Error with context
- [ ] Integration with logging service (e.g., ELK, DataDog)
- [ ] **Test**: Verify logs appear in monitoring system

### 7.2 Metrics & Health Checks
- [ ] Prometheus-style metrics endpoint
  - Request counts by endpoint
  - Token usage by provider
  - Plugin execution times
- [ ] Health check endpoint (`/health`)
- [ ] **Test**: Scrape metrics, verify completeness

### 7.3 Tracing
- [ ] Distributed tracing (OpenTelemetry)
  - Trace request through Core → Provider → Plugin → Response
- [ ] **Test**: View trace in observability UI

---

## Phase 8: Performance & Optimization

### 8.1 Caching
- [ ] Implement response caching (Redis or in-memory)
  - Cache LLM responses for identical prompts
  - Cache plugin metadata
- [ ] **Test**: Measure response time improvement

### 8.2 Database Optimization
- [ ] Index frequently queried columns (user_id, conversation_id, created_at)
- [ ] Query optimization (N+1 prevention)
- [ ] Connection pooling tuning
- [ ] **Test**: Load test with 1000 concurrent users

### 8.3 Streaming Optimization
- [ ] Profile token streaming latency
- [ ] Batch token sends if beneficial
- [ ] Memory usage under load
- [ ] **Test**: 100+ concurrent streams

---

## Phase 9: Testing & Quality

### 9.1 Unit Tests
- [ ] Core domain logic (90%+ coverage)
- [ ] Provider implementations
- [ ] Plugin executor
- [ ] Message orchestration
- [ ] **Test**: Run `dotnet test`

### 9.2 Integration Tests
- [ ] API endpoints (happy path + error cases)
- [ ] Database migrations
- [ ] Provider integration (with mocks)
- [ ] **Test**: Run integration suite

### 9.3 End-to-End Tests
- [ ] Full conversation flow (UI → API → Provider → UI)
- [ ] Multi-user scenarios
- [ ] **Test**: Automated E2E suite

---

## Phase 10: Documentation & Release

### 10.1 API Documentation
- [ ] OpenAPI/Swagger spec
- [ ] Generated docs site
- [ ] Example requests/responses
- [ ] Error codes reference

### 10.2 SDK Documentation
- [ ] Getting started guides per language
- [ ] API reference
- [ ] Plugin development guide
- [ ] Example plugins

### 10.3 Deployment Guide
- [ ] Docker/Podman setup
- [ ] Configuration reference
- [ ] Environment variables
- [ ] Database migration instructions

### 10.4 v1.0 Release
- [ ] Tag release
- [ ] Publish SDKs to package managers (NuGet, PyPI, npm)
- [ ] Announce to community

---

## Quick Reference: Blocked Checklist

Use this section as a quick checklist for current work:

### Current Sprint
- [ ] Section 1.1 (Model Provider Abstraction)
- [ ] Section 1.2 (Token Streaming Pipeline)
- [ ] Section 1.3 (Message Metadata)
- [ ] Section 1.4 (Error Handling)

### Next Sprint
- [ ] Section 2.1 (Plugin Registry)
- [ ] Section 2.2 (Plugin Contract)
- [ ] Section 2.3 (Plugin Invocation)

### Backlog
- Everything in Phases 3–10

---

## Notes for the Agent

- **Preserve functional behavior:** Each phase should maintain conversation lifecycle and message persistence.
- **Functional style:** Keep domain models immutable; express logic as `State → Command → Result<State>`.
- **No infrastructure leakage:** Domain layer must remain provider-agnostic.
- **Test-driven:** Write tests *before* implementing each section.
- **Documentation:** Update docs alongside code changes.
- **Docker-first:** Ensure each phase works in Docker Compose locally before moving on.

---

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| 2026-03-17 | 1.0 | Initial roadmap created |
