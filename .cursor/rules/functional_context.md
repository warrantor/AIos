---
apply: always
---

# Functional Context

This file is intended to describe **what the system does** from a product and workflow standpoint.
For implementation constraints and replacement architecture, also load the technical context section below in this file.

## Purpose

<span style="font-family: sans;color: violet;">AIos</span> is a self-hostable, API-first orchestration platform for LLM/chat workflows.
Its job is to sit between users/clients and model/tooling infrastructure, persist the interaction state, stream responses in real time, and expose the platform through stable APIs.

## Primary User Jobs

Users and client applications use the platform to:

- create and continue conversations
- send prompts/messages into LLM-backed workflows
- receive streamed responses token-by-token
- inspect previous conversation and message history
- invoke tools, plugins, or extensions through the platform
- manage automation-related data
- interact through multiple clients such as web UI and CLI

## Functional Capabilities

### 1. Conversation Management

The system must support the lifecycle of a conversation:

- create a conversation
- resume an existing conversation
- append new messages
- retrieve prior conversation state
- maintain the ordering and association of messages within a conversation

### 2. Message History Persistence

The system must persist:

- user messages
- assistant/model messages
- metadata associated with messages and conversations
- enough history to support replay, continuation, and inspection

### 3. LLM Request Orchestration

The platform coordinates requests to LLM-backed workflows.
This includes:

- accepting a user request
- shaping it into the internal application flow
- invoking provider/model/tooling logic
- handling the response lifecycle
- returning either full or streamed responses to clients

### 4. Real-Time Token Streaming

Responses should be streamable as they are generated.
Expected behavior:

- partial response units are emitted progressively
- clients can render output incrementally
- the system can signal completion, failure, or interruption
- the streaming path is treated as a first-class capability, not an afterthought

### 5. Plugin / Addon / Tool Extensibility

The platform supports extensions so that capabilities are not hardcoded into a single monolith.
Functionally, this means:

- tools/plugins/addons can be described and discovered
- plugin-related metadata can be stored
- the core platform can coordinate with extensions without being tightly coupled to them

### 6. Automation Data Handling

The system contains automation-related capabilities or metadata.
At minimum, it must support:

- storing automation-related records
- associating them with users/system state where relevant
- exposing them through APIs or workflows that need them

### 7. User Data Management

The platform manages user-related information needed for operation.
This includes:

- user-associated records
- state needed for personalization, ownership, or access behavior
- durable storage and retrieval of user-scoped data

### 8. Multi-Client Access

The platform is not tied to a single interface.
It should support:

- browser-based UI usage
- CLI usage
- API/SDK-based access patterns
  The core behavior should remain consistent across these client types.

## Core Functional Domains

The main data/function domains are:

- conversations
- message history
- plugin metadata
- automations
- user data

## End-to-End Functional Flow

Typical request flow:

1. A user or client starts/resumes a conversation
2. A message/prompt is submitted
3. The platform orchestrates the request through its application logic
4. Provider/tool/plugin logic is invoked as needed
5. Output is persisted and/or streamed
6. The client receives partial or complete results
7. Conversation state remains available for later continuation

## Functional Boundaries

Preserve these behaviors in the replacement:

- conversation/message lifecycle
- persistent history
- token streaming
- modular extensibility
- API-first access
- real-time updates
- support for multiple clients (web, CLI, SDK-style consumers)

Do not preserve these as functional requirements:

- original web component implementation
- exact old module/project layout
- edge transport implementation details if equivalent behavior is maintained

## Guidance for Coding Agents

When using this file as project context:

1. Read **Functional Context** first to understand the product behavior.
2. Then read **Technical / Architecture Context** in the same file to understand target implementation constraints.
3. Prefer preserving workflows and behavior over copying implementation details.

## More context

Include [project_context](./project_context.md) for a more technical overview.
Current state of implementation you will find in the [roadmap](./implementation_roadmap.md)
