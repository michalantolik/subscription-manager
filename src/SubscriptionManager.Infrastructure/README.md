# SubscriptionManager.Infrastructure

The Infrastructure project contains implementations of application contracts using external systems, persistence, and framework-specific services.

## Infrastructure Areas

| Area            | Responsibility                                                                                   |
| --------------- | ------------------------------------------------------------------------------------------------ |
| Authentication  | Provides JWT access token generation and authentication-related email delivery.                  |
| Billing         | Integrates billing workflows with Stripe and persists billing state and webhook processing data. |
| DigitalServices | Persists digital services and provides their initial catalog data.                               |
| ExchangeRates   | Retrieves exchange rates from NBP and persists exchange rate data.                               |
| SavingsPlans    | Integrates savings plan generation with OpenAI and persists usage data.                          |
| Subscriptions   | Persists user subscriptions and their relationships with digital services.                       |

## Common

Contains infrastructure-wide implementations shared across multiple application areas.

| Area     | Responsibility                                                                      |
| -------- | ----------------------------------------------------------------------------------- |
| Identity | Implements shared user identity and account operations using ASP.NET Core Identity. |

## Persistence

Provides the EF Core database context, database initialization, and migrations.
