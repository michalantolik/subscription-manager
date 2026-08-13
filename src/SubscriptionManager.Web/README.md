# SubscriptionManager.Web

The Web project provides the user-facing Blazor Server application and communicates with the Subscription Manager API over HTTP.

## Web Areas

| Area            | Responsibility                                                                                 |
|-----------------|------------------------------------------------------------------------------------------------|
| Account         | Provides account settings, preferences, and account lifecycle interactions.                    |
| Authentication  | Provides registration, login, email confirmation, password recovery, and web session handling. |
| Billing         | Provides plan selection, billing status, subscription changes, cancellation, and resumption.   |
| DigitalServices | Provides digital service data and custom digital service creation used by subscription flows.  |
| Overview        | Provides the application dashboard and recurring cost overview.                                |
| SavingsPlans    | Provides savings plan generation, usage information, and savings scenarios.                    |
| Subscriptions   | Provides subscription management, filtering, summaries, and recurring cost presentation.       |

## Common

Contains web-wide types and services shared across multiple application areas.

| Area           | Responsibility                                                                |
|----------------|-------------------------------------------------------------------------------|
| Api            | Provides configuration for communication with the backend API.                |
| Currencies     | Provides currencies shared by web features and API contracts.                 |
| FeatureToggles | Provides configuration-backed feature availability checks.                    |
| Localization   | Provides supported languages, cultures, and localized UI text.                |
| State          | Provides shared UI state and notifications used across the web application.   |

## Components

Contains the application shell and UI components that are not owned by a single business area, including the main layout, navigation, shared icons, routing, and application-level error pages.
