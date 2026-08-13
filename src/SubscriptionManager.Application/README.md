# SubscriptionManager.Application

The Application project contains the use cases and workflows of Subscription Manager.

## Application Areas

| Area            | Responsibility                                                                                   |
|-----------------|--------------------------------------------------------------------------------------------------|
| Account         | Allows users to manage their account preferences and account lifecycle.                          |
| Authentication  | Handles registration, login, email confirmation, and password recovery.                          |
| Billing         | Handles Subscription Manager plan changes, checkout, cancellation, and billing status.           |
| DigitalServices | Allows users to manage the digital services associated with their subscriptions.                 |
| ExchangeRates   | Provides supporting functionality for subscription cost conversion using current exchange rates. |
| SavingsPlans    | Generates personalized savings plans and manages their usage limits.                             |
| Subscriptions   | Allows users to create, update, view, end, and delete their subscriptions.                       |

## Common

Contains application-wide contracts and types shared across multiple application areas.

| Area         | Responsibility                                                   |
|--------------|------------------------------------------------------------------|
| Identity     | Provides shared access to user identity and identity operations. |
| Localization | Provides supported languages and language-related conversions.   |
