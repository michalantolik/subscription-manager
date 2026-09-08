# AGENTS.md

## Purpose

This repository contains Subscription Manager, a .NET web application for managing recurring subscriptions and their costs.

Keep changes practical, maintainable and consistent with the existing architecture. Prefer clear product code over speculative abstractions.

## Source of Truth

Use these files before making non-trivial changes:

- `README.md` — product overview, local setup and repository structure;
- `docs/architecture.md` — architectural boundaries and dependency direction;
- `docs/decisions.md` — durable engineering decisions;
- `docs/testing.md` — testing strategy and validation expectations;
- `docs/measurement.md` — aggregate product and technical measurement rules;
- `docs/measurement-snapshots.md` — weekly historical measurement snapshot contract;
- `docs/learnings.md` — reusable engineering lessons;
- `docs/roadmap.md` — technical and product-quality follow-up work.

Project-level README files under `src/` own feature/module structure for their respective projects. `infra/README.md` owns infrastructure setup and deployment prerequisites.

## Working Rules

- preserve the existing Domain → Application → Infrastructure/API/Web boundaries;
- keep domain and application code independent of infrastructure details;
- prefer feature-oriented code that matches the existing repository structure;
- avoid MediatR or additional architectural layers unless a demonstrated problem requires them;
- add tests where behavior, regressions or integration boundaries justify them;
- do not add tests solely to increase coverage;
- keep comments focused on non-obvious reasoning rather than restating code;
- keep secrets out of source control;
- update documentation when a durable decision, operating rule or repository structure changes;
- keep public documentation factual, concise and useful to someone evaluating or contributing to the project.

## Validation

For meaningful code changes, use the smallest relevant checks and expand when the change crosses boundaries.

Typical repository-level validation:

```powershell
dotnet restore SubscriptionManager.slnx
dotnet build SubscriptionManager.slnx --configuration Release --no-restore
dotnet test SubscriptionManager.slnx --configuration Release --no-build
dotnet format SubscriptionManager.slnx --verify-no-changes
```

Infrastructure changes should also follow the validation documented in `infra/README.md` and the existing infrastructure workflow.

## Measurement History

Treat weekly measurement snapshots as factual historical evidence.

- preserve stable metric definitions;
- use the shared weekly cadence once its exact schedule is activated;
- keep facts separate from interpretation;
- represent unavailable data explicitly rather than as zero;
- use aggregate product and technical signals;
- do not commit secrets, raw telemetry or unnecessary personal data;
- do not infer causality from timing alone;
- update the measurement contract when a metric definition materially changes.

Measurement should help evaluate product health and changes without becoming a reason to add unnecessary instrumentation.
