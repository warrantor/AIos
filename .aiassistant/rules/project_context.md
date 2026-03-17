---
apply: always
---

# Technical / Architecture Context
Check [functional_context](./functional_context.md) for how the app should behave.
Current state of implementation you will find in the [roadmap](./implementation_roadmap.md)

## Solution structure (AIos.slnx)

Overview of the solution layout and projects for prompt context.
When this changes, update the section.

## Folder hierarchy

| Folder | Contents |
|--------|----------|
| **0. Solution Items** | Root-level files (e.g. `compose.yaml`) |
| **Addons/SDK** | Client SDKs: Dotnet, Python, TypeScript |
| **Core** | Domain, persistence, application layer |
| **Gateway** | HTTP/WebSocket API, references Core |
| **UI** | CLI and Web frontends |

## Projects

| Project | Path | Target | Purpose |
|---------|------|--------|---------|
| **AIos.Core** | `AIos.Core/AIos.Core.csproj` | net10.0 | Domain models, persistence, application orchestration. No infra dependencies in domain. |
| **AIos.Gateway** | `AIos.Gateway/AIos.Gateway.csproj` | net10.0 (Web) | REST + WebSocket API; Docker image `aios-gateway`. |
| **AIos.Sdk** | `Addons/SDK/Dotnet/src/AIos.Sdk/AIos.Sdk.csproj` | net10.0 | .NET client SDK for the platform (addon). |
| **AIos.Cli** | `UI/CLI/src/AIos.Cli/AIos.Cli.csproj` | net10.0 (Exe) | CLI entrypoint; assembly name `aios`. No Core reference. |

## Packages and dependencies

**AIos.Core**

- **Project references:** none
- **NuGet:** `Microsoft.EntityFrameworkCore.Design` 10.0.5 (build-only, PrivateAssets); `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1

**AIos.Gateway**

- **Project references:** `AIos.Core`
- **NuGet:** `FastEndpoints` 8.0.1

**AIos.Sdk**

- **Project references:** none
- **NuGet:** none

**AIos.Cli**

- **Project references:** none
- **NuGet:** `Spectre.Console.Cli` 0.53.1

## Placeholders / empty

- **Addons/SDK/Python/src**, **Addons/SDK/Typescript/src** — SDK projects not yet added to solution.
- **UI/Web/src** — Web UI (e.g. Next.js) not yet in slnx.

## Run / deploy

- **Docker**: `compose.yaml` builds and runs `aios-gateway` from `AIos.Gateway/Dockerfile`.
- **CLI**: build/run `AIos.Cli`; executable name `aios`.

---

Important rules:

- domain models must not depend on infrastructure
- persistence logic belongs in core
- application layer (lives in core) orchestrates domain logic

---

## Functional Programming Style

Although written in C#, the codebase prefers a **functional style** where possible.

Guidelines:

Prefer:

- immutable data
- pure functions
- explicit state transitions
- value objects
- composition over inheritance

Avoid:

- deep inheritance hierarchies
- mutable shared state
- overly complex object graphs

Domain behavior should often be expressed as:
```text
DomainState -> Command -> Result<DomainState>
```

---

# Data Storage

Primary database:

**PostgreSQL**

Used for:

- conversations
- message history
- plugin metadata
- automations
- user data

Design principles:

- schema migrations must be versioned
- domain entities should map cleanly to persistence models
- avoid leaking database concerns into the domain layer

---

# Communication

Internal communication patterns:

| Method     | Purpose                             |
|------------|-------------------------------------|
| REST       | standard API operations             |
| WebSockets | real-time LLM streaming, UI updates |
| Channels   | internal event subscriptions        |

---

# Streaming

LLM responses must support **token streaming**.

Flow:
```text
LLM Provider
↓
Core Orchestrator
↓
Gateway
↓
WebSocket (SignalR)
↓
UI
```

---

# Deployment

Target environments:

- Docker
- Podman

The system should support:

- local development
- self-hosting
- cloud deployment

---

# Performance Goals

AIos is designed to outperform typical Node.js-based LLM orchestration platforms.

Strategies include:

- .NET JIT / AOT
- minimal allocations
- streaming pipelines
- efficient concurrency

---

# Non-Goals

The project does NOT aim to:

- tightly couple plugins to the core
- create a monolithic architecture
- require a single programming language for extensions

---

# Coding Guidelines

General rules:

- prefer small composable modules
- explicit types over dynamic structures
- avoid unnecessary abstractions
- favor readability and maintainability
- write unit tests where complex logic is involved

Naming:

- domain terminology must reflect the ubiquitous language
- avoid generic names like `Manager`, `Helper`, or `Utils`

---

# Summary

AIos is a modular LLM platform built around:

- **.NET 10 Core Orchestrator**
- **Next.js UI**
- **API-first plugin ecosystem**
- **DDD architecture**
- **functional programming style**
- **PostgreSQL persistence**

The goal is to create a fast, extensible, and developer-friendly alternative to existing bot platforms.