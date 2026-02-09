# TextToCode

AI-powered C# code generation engine with a self-healing pipeline that validates, compiles, and safely executes generated code.

## Features

- Self-healing pipeline: generate -> validate -> compile -> execute, with diagnostics fed back for retries.
- Roslyn compilation + AST safety validation for dangerous APIs.
- Sandboxed execution with timeout controls.
- Console REPL and REST API + SignalR streaming.
- Clean architecture with Core, Application, Infrastructure, and Presentation layers.

## Architecture

Pipeline flow:

User Prompt -> Generate (LLM) -> Validate (AST walker) -> Compile (Roslyn) -> Execute (sandbox)
    ^                                                                |
    |----------------- on error: feed diagnostics back --------------|

Key patterns:
- Strategy: swappable `ILlmClient`, `ICodeCompiler`
- Visitor: `DangerousApiWalker`
- Pipeline: self-healing loop
- Observer: `IProgress<T>`
- Options pattern
- Collectible `AssemblyLoadContext` for sandboxed execution

## Solution Structure

Projects in `TextToCode.slnx`:

- Core: `TextToCode.Core` (interfaces, entities, value objects, enums, exceptions)
- Application: `TextToCode.Application` (pipeline, services, DTOs)
- Infrastructure: `TextToCode.Infrastructure` (Roslyn compiler, safety validator, sandbox, OpenRouter client)
- Presentation: `TextToCode.Console` (CLI + REPL)
- Presentation: `TextToCode.WebApi` (REST API + SignalR + Swagger)
- Tests: `TextToCode.Core.Tests`, `TextToCode.Application.Tests`, `TextToCode.Infrastructure.Tests`

## Prerequisites

- .NET SDK `10.0.102` (see `global.json`)
- OpenRouter API key

## Configuration

Both Console and Web API read settings from `appsettings.json`.

OpenRouter options:
- `OpenRouter:ApiKey`
- `OpenRouter:BaseUrl` (default: `https://openrouter.ai/api/v1`)
- `OpenRouter:Model` (default: `anthropic/claude-sonnet-4`)
- `OpenRouter:Temperature`
- `OpenRouter:MaxTokens`

Pipeline options:
- `Pipeline:MaxRetries`
- `Pipeline:ExecutionTimeoutSeconds`

Environment variables:
- Console uses prefix `TEXTTOCODE_`, e.g. `TEXTTOCODE_OpenRouter__ApiKey=...`
- Web API uses standard ASP.NET Core config keys, e.g. `OpenRouter__ApiKey=...`

## Usage

### Console CLI

Generate code once:

```
dotnet run --project src/TextToCode.Console generate "Write a C# method that computes Fibonacci"
```

Interactive REPL (default when no command specified):

```
dotnet run --project src/TextToCode.Console
```

REPL commands:
- `exit` or `quit` to leave
- `clear` to clear the screen

### Web API

Run the API:

```
dotnet run --project src/TextToCode.WebApi
```

Endpoints:
- `GET /api/health`
- `POST /api/codegen/generate`
- SignalR hub: `/hubs/codegen`

Example request:

```
curl -X POST http://localhost:5000/api/codegen/generate \
  -H "Content-Type: application/json" \
  -d "{\"prompt\":\"Write a C# method that computes Fibonacci\"}"
```

SignalR events:
- `ProgressUpdate` (streamed status)
- `GenerationComplete` (final response)

## Tests

```
dotnet test TextToCode.slnx --verbosity normal
```

## Notes

- Default max retries: 3 (total attempts = `MaxRetries + 1`)
- Execution timeout defaults to 10 seconds
