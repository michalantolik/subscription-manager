# Subscription Manager

[![CI](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml)

A web application for managing recurring subscriptions for services such as Netflix, Spotify and ChatGPT Plus.

## Features

- Create, edit and delete subscriptions
- Manage predefined and custom digital services
- Track recurring monthly and yearly costs
- Generate personalized savings plans
- Manage Free, Plus and Premium billing plans
- REST API with a Blazor web client

## Technology

- .NET 10
- ASP.NET Core Web API
- Blazor Server
- Entity Framework Core
- SQL Server
- [Terraform](infra/README.md)

## External integrations

| Service                                   | Purpose                                         |
|-------------------------------------------|-------------------------------------------------|
| [Stripe API](https://docs.stripe.com/api) | Subscription billing and payment processing     |
| [NBP API](https://api.nbp.pl/)            | Exchange rates for subscription cost conversion |
| [OpenAI API](https://openai.com/api/)     | Personalized savings plan generation            |

## Project structure

The solution is organized into business areas across its architectural layers.

Columns show corresponding areas rather than project dependency direction.

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

## Status

The project is under active development.
