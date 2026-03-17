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

⚠️ Update when changed.

| Folder                | Contents                                |
| --------------------- | --------------------------------------- |
| **0. Solution Items** | Root-level files (e.g. `compose.yaml`)  |
| **Addons/SDK**        | Client SDKs: Dotnet, Python, TypeScript |
| **Core**              | Domain, persistence, application layer  |
| **Gateway**           | HTTP/WebSocket API, references Core     |
| **UI**                | CLI and Web frontends                   |

## Projects

| Project          | Path                                             | Target        | Purpose                                                                                 |
| ---------------- | ------------------------------------------------ | ------------- | --------------------------------------------------------------------------------------- |
| **AIos.Core**    | `AIos.Core/AIos.Core.csproj`                     | net10.0       | Domain models, persistence, application orchestration. No infra dependencies in domain. |
| **AIos.Gateway** | `AIos.Gateway/AIos.Gateway.csproj`               | net10.0 (Web) | REST + WebSocket API; Docker image `aios-gateway`.                                      |
| **AIos.Sdk**     | `Addons/SDK/Dotnet/src/AIos.Sdk/AIos.Sdk.csproj` | net10.0       | .NET client SDK for the platform (addon).                                               |
| **AIos.Cli**     | `UI/CLI/src/AIos.Cli/AIos.Cli.csproj`            | net10.0 (Exe) | CLI entrypoint; assembly name `aios`. No Core reference.                                |

## Packages and dependencies

⚠️ Keep in sync with the project.

**[AIos.Core](../../AIos.Core/)**

- **Project references:** [AIos.Sdk](../../AIos.Sdk/)
- **NuGet:**

| Package                                   | Version  | Notes                          |
| ----------------------------------------- | -------- | ------------------------------ |
| `Microsoft.EntityFrameworkCore.Design`    | 10.0.5   | build-only, PrivateAssets      |
| `Microsoft.Extensions.AI`                 | 10.4.0   |                                |
| `Npgsql.EntityFrameworkCore.PostgreSQL`    | 10.0.1   |                                |

**[AIos.Gateway](../../AIos.Gateway/)**

- **Project references:** [AIos.Core](../../AIos.Core/), [AIos.Sdk](../../AIos.Sdk/)
- **NuGet:**

| Package                                | Version  |
| -------------------------------------- | -------- |
| `FastEndpoints`                        | 8.0.1    |
| `Microsoft.Extensions.AI.OpenAI`       | 10.4.0   |
| `Microsoft.Extensions.Configuration.UserSecrets` | 10.0.5   |

**[AIos.Sdk](../../AIos.Sdk/)**

- **Project references:** none
- **NuGet:** none

**[AIos.Cli](../../AIos.Cli/)**

- **Project references:** [AIos.Sdk](../../AIos.Sdk/)
- **NuGet:**

| Package                | Version |
| ---------------------- | ------- |
| `Spectre.Console.Cli`  | 0.53.1  |

**[AIos.Web](../../AIos.Web/)** (pnpm)

| Package              | Version   | Type     |
| -------------------- | --------- | -------- |
| `@t3-oss/env-nextjs` | ^0.12.0   | dependency |
| `@tanstack/react-query` | ^5.69.0 | dependency |
| `@trpc/client`       | ^11.0.0   | dependency |
| `@trpc/react-query`  | ^11.0.0   | dependency |
| `@trpc/server`       | ^11.0.0   | dependency |
| `better-auth`        | ^1.3      | dependency |
| `next`               | ^15.2.3   | dependency |
| `react`              | ^19.0.0   | dependency |
| `react-dom`          | ^19.0.0   | dependency |
| `server-only`        | ^0.0.1    | dependency |
| `superjson`          | ^2.2.1    | dependency |
| `zod`                | ^3.24.2   | dependency |
| `@eslint/eslintrc`   | ^3.3.1    | devDependency |
| `@tailwindcss/postcss` | ^4.0.15 | devDependency |
| `@types/node`        | ^20.14.10 | devDependency |
| `@types/react`       | ^19.0.0   | devDependency |
| `@types/react-dom`   | ^19.0.0   | devDependency |
| `eslint`             | ^9.23.0   | devDependency |
| `eslint-config-next` | ^15.2.3   | devDependency |
| `postcss`            | ^8.5.3    | devDependency |
| `prettier`           | ^3.5.3    | devDependency |
| `prettier-plugin-tailwindcss` | ^0.6.11 | devDependency |
| `tailwindcss`        | ^4.0.15   | devDependency |
| `typescript`         | ^5.8.2    | devDependency |
| `typescript-eslint`  | ^8.27.0   | devDependency |

## Placeholders / empty

- **Addons/SDK/Python/src**, **Addons/SDK/Typescript/src** — SDK projects not yet added to solution.
- **UI/Web/src** — Web UI (e.g. Next.js) not yet in slnx.

## Run / deploy

- **Docker**: `compose.yaml` builds and runs `aios-gateway` from `AIos.Gateway/Dockerfile`.
- **CLI**: build/run `AIos.Cli`; executable name `aios`.

### Package documentation

| Name                        | URL                                                                                                   |
| --------------------------- | ----------------------------------------------------------------------------------------------------- |
| **Microsoft.Extensions.AI** | [.NET + AI ecosystem tools and SDKs](https://learn.microsoft.com/en-us/dotnet/ai/dotnet-ai-ecosystem) |

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

Respect the [.editorconfig](../../.editorconfig).

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
| ---------- | ----------------------------------- |
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
