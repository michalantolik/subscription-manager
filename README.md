# Subscription Manager

[![CI](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml)
[![Deploy](https://github.com/michalantolik/subscription-manager/actions/workflows/deploy.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/deploy.yml)
[![Infrastructure](https://github.com/michalantolik/subscription-manager/actions/workflows/infrastructure.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/infrastructure.yml)

A web application for managing recurring subscriptions for services such as Netflix, Spotify and ChatGPT Plus.

## Features

- Create, edit and delete subscriptions
- Manage predefined and custom digital services
- Track recurring monthly and yearly costs
- Generate personalized savings plans
- Manage Free, Plus and Premium billing plans
- Available in Polish, English and German

## Technologies

- .NET 10
- ASP.NET Core Web API
- Blazor Server
- Entity Framework Core
- SQL Server
- Azure
- Terraform
- xUnit

## External integrations

| Service                                   | Purpose                                         |
|-------------------------------------------|-------------------------------------------------|
| [Stripe API](https://docs.stripe.com/api) | Subscription billing and payment processing     |
| [NBP API](https://api.nbp.pl/)            | Exchange rates for subscription cost conversion |
| [OpenAI API](https://openai.com/api/)     | Personalized savings plan generation            |

## Architecture

```text
Web --HTTP--> API

API -------------> Application
API -------------> Infrastructure

Infrastructure --> Application
Infrastructure --> Domain

Application -----> Domain
```

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