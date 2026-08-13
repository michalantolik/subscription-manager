# Subscription Manager

[![CI](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/michalantolik/subscription-manager/actions/workflows/ci.yml)

A web application for managing recurring subscriptions and digital services such as Netflix, Spotify and ChatGPT Plus.

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

## External integrations

| Service    | Purpose                                          |
|------------|--------------------------------------------------|
| Stripe     | Subscription billing and payment processing      |
| NBP API    | Exchange rates for subscription cost conversion  |
| OpenAI API | Personalized savings plan generation             |

## Project structure

The backend is organized into business areas that span the architectural layers where required.

| Domain            |   | Application        |   | Infrastructure     |   | API                |
|-------------------|---|--------------------|---|--------------------|---|--------------------|
| `Billing`         | ↔ | `Billing`          | ↔ | `Billing`          | ↔ | `Billing`          |
| `DigitalServices` | ↔ | `DigitalServices`  | ↔ | `DigitalServices`  | ↔ | `DigitalServices`  |
| `SavingsPlans`    | ↔ | `SavingsPlans`     | ↔ | `SavingsPlans`     | ↔ | `SavingsPlans`     |
| `Subscriptions`   | ↔ | `Subscriptions`    | ↔ | `Subscriptions`    | ↔ | `Subscriptions`    |
|                   |   | `Account`          |   |                    | ↔ | `Account`          |
|                   |   | `Authentication`   | ↔ | `Authentication`   | ↔ | `Authentication`   |
| `ExchangeRates`   | ↔ | `ExchangeRates`    | ↔ | `ExchangeRates`    |   |                    |

The Blazor project provides the web client for the API.

## Status

The project is under active development.
