# Architecture

Subscription Manager uses a layered .NET architecture with feature-oriented organization inside each project.

```text
Web ──HTTP──► API
                │
                ├──► Application ──► Domain
                │
                └──► Infrastructure ──► Application + Domain
```

## Projects

### Domain

Owns core business concepts and rules such as subscriptions, billing plans, digital services, currencies and savings-plan usage.

It must not depend on application, infrastructure, API or web concerns.

### Application

Owns use cases, commands, handlers, DTOs and interfaces required by those use cases.

Infrastructure dependencies are expressed as application-owned abstractions.

### Infrastructure

Implements persistence and external integrations, including SQL Server/Entity Framework Core, identity infrastructure, Stripe, NBP exchange rates, email delivery and OpenAI-backed savings-plan generation.

### API

Exposes application capabilities over HTTP, configures the application host and maps API-specific concerns such as controllers and exception handling.

### Web

Provides the Blazor user interface and communicates with the API over HTTP.

## Dependency Direction

Keep dependencies pointing toward the business core:

```text
Domain
  ▲
  │
Application
  ▲
  │
Infrastructure

API ──► Application + Infrastructure
Web ──HTTP──► API
```

Do not move provider-specific logic into Domain or Application merely to reduce file count.

## Change Guidance

Prefer extending an existing feature slice when the behavior belongs to an existing capability. Introduce a new shared abstraction only when at least two real consumers need the same concept and the abstraction makes the dependency clearer.

Cross-cutting behavior should remain small and explicit. Avoid adding framework patterns solely for architectural symmetry.
