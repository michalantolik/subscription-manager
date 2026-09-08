# Subscription Manager

[![CI](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml)
[![Deploy](https://github.com/michalantolik/subscription-manager/actions/workflows/deploy.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/deploy.yml)
[![Infrastructure](https://github.com/michalantolik/subscription-manager/actions/workflows/infrastructure.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/infrastructure.yml)

A web application for managing recurring subscriptions and tracking their costs.

**Live application:** [submanager.dev](https://submanager.dev)

## Features

- Create, edit and delete subscriptions
- Manage predefined and custom digital services
- Track recurring monthly and yearly costs
- Generate personalized savings plans
- Compare Free, Plus and Premium plans
- Available in Polish, English and German

## Screenshot

[![Subscription cost overview](docs/images/dashboard.png)](docs/images/dashboard.png)

## Technologies

```text
.NET 10
│
├── Web ─────────────► Blazor Server
├── API ─────────────► ASP.NET Core Web API
├── Persistence ─────► Entity Framework Core ──► SQL Server
├── Infrastructure ──► Terraform ──► Azure
└── Tests ───────────► xUnit
```

## Architecture

```text
Web ──HTTP──► API
                │
                ├──► Application ──► Domain
                │
                └──► Infrastructure ──► Application + Domain
```

## External integrations

| Service                                   | Purpose                                         |
|-------------------------------------------|-------------------------------------------------|
| [Stripe API](https://docs.stripe.com/api) | Subscription billing and payment processing     |
| [NBP API](https://api.nbp.pl/)            | Exchange rates for subscription cost conversion |
| [OpenAI API](https://openai.com/api/)     | Personalized savings plan generation            |

## Running locally

### Requirements

- .NET 10 SDK
- Visual Studio 2026 with the ASP.NET and web development workload
- SQL Server Express LocalDB

### Setup

1. Clone the repository and open `SubscriptionManager.slnx`.
2. Select the `Web + API` launch profile and run the solution.

The API applies database migrations and seeds the digital service catalog on first startup.<br>
After registration, use the confirmation link written to the API output.

No API keys are required for the core application features.<br>
To enable savings plan generation, configure an OpenAI API key:

```powershell
dotnet user-secrets set "SavingsPlanAi:ApiKey" "<api-key>" --project src/SubscriptionManager.Api
```

## Documentation

| Document | Purpose |
|---|---|
| [Architecture](docs/architecture.md) | Project boundaries and dependency direction |
| [Engineering decisions](docs/decisions.md) | Durable architectural and operating decisions |
| [Testing](docs/testing.md) | Testing priorities and repository validation |
| [Measurement](docs/measurement.md) | Aggregate product and technical measurement rules |
| [Measurement snapshots](docs/measurement-snapshots.md) | Weekly historical measurement contract |
| [Engineering learnings](docs/learnings.md) | Reusable lessons from implementation and operations |
| [Roadmap](docs/roadmap.md) | Current repository-quality follow-up work |

## Repository archive

To create a clean ZIP archive of the repository while preserving the complete Git history, run:

```powershell
.\scripts\archive-repository.cmd
```

Before archiving, the script fetches the configured upstream and checks whether the current branch is up to date. If the working tree is clean and the branch is only behind its upstream, it performs a safe fast-forward-only pull. It never pushes, stashes, merges or rebases automatically. If local changes exist, the pull is skipped and those changes are preserved in the archive.

The archive is created one directory above the repository and uses the repository name, for example `subscription-manager.zip`. An existing archive with the same name is replaced. The script stages a temporary copy, excludes local build and test artifacts, preserves `.git`, verifies the resulting archive and otherwise leaves the working tree unchanged.

The archive contains the complete Git history and may include sensitive historical or local data. Keep it private.

## Project structure

| [Domain](src/SubscriptionManager.Domain/README.md) | [Application](src/SubscriptionManager.Application/README.md) | [Infrastructure](src/SubscriptionManager.Infrastructure/README.md) | [API](src/SubscriptionManager.Api/README.md) | [Web](src/SubscriptionManager.Web/README.md) |
|----------------------------------------------------|----------------------------------------------------------------|-------------------------------------------------------------------|------------------------------------------|------------------------------------------|
| `Billing`                                          | `Billing`                                                      | `Billing`                                                         | `Billing`                                | `Billing`                                |
| `DigitalServices`                                  | `DigitalServices`                                              | `DigitalServices`                                                 | `DigitalServices`                        | `DigitalServices`                        |
| `SavingsPlans`                                     | `SavingsPlans`                                                 | `SavingsPlans`                                                    | `SavingsPlans`                           | `SavingsPlans`                           |
| `Subscriptions`                                    | `Subscriptions`                                                | `Subscriptions`                                                   | `Subscriptions`                          | `Subscriptions`                          |
|                                                    | `Account`                                                      |                                                                   | `Account`                                | `Account`                                |
|                                                    | `Authentication`                                               | `Authentication`                                                  | `Authentication`                         | `Authentication`                         |
| `ExchangeRates`                                    | `ExchangeRates`                                                | `ExchangeRates`                                                   |                                          |                                          |
|                                                    |                                                                |                                                                   |                                          | `Overview`                               |
