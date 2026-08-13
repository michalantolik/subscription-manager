# SubscriptionManager.Api

The API project exposes Subscription Manager use cases through HTTP endpoints and configures the application host.

## API Areas

| Area            | Responsibility                                                                                     |
|-----------------|----------------------------------------------------------------------------------------------------|
| Account         | Exposes account preference management.                                                             |
| Authentication  | Exposes registration, login, email confirmation, password recovery, and user identity operations.  |
| Billing         | Exposes plan information, checkout, subscription changes, and payment webhooks.                    |
| DigitalServices | Exposes digital service management.                                                                |
| SavingsPlans    | Exposes savings plan generation and usage information.                                             |
| Subscriptions   | Exposes subscription management and recurring cost summaries.                                      |

## Common

Contains API-wide components shared across multiple API areas.

| Area              | Responsibility                                                         |
|-------------------|------------------------------------------------------------------------|
| ExceptionHandling | Maps application exceptions to HTTP problem details responses.         |
| Identity          | Provides the current authenticated user from the HTTP context.         |

## Host

Configures authentication, authorization, rate limiting, exception handling, serialization, OpenAPI, health checks, and the application request pipeline.
