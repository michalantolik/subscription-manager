# Measurement

Subscription Manager measurement should answer a small set of product-health and technical-health questions without collecting more data than the decision requires.

## Principles

- prefer aggregate signals;
- define every metric before using it for decisions;
- preserve counts alongside rates when denominators matter;
- distinguish acquisition, activation, continued use, billing and reliability;
- treat small samples cautiously;
- separate operator/test traffic where practical;
- keep personal data and raw telemetry out of repository history;
- do not treat correlation as causation.

## Initial Scorecard

Use only metrics that can be sourced reliably.

### Acquisition

Examples:

- visits to the public application;
- registration starts;
- completed registrations.

### Product Use

Examples:

- active accounts for the period;
- accounts with at least one subscription;
- subscriptions created;
- overview/dashboard usage when measured reliably;
- savings-plan generation attempts and successful results.

### Billing

Examples:

- plan distribution;
- checkout starts;
- completed paid-plan activations;
- cancellations.

Billing metrics must be interpreted with their denominators and without exposing customer-level payment data.

### Technical Health

Examples:

- availability;
- request failures;
- server errors;
- relevant response-time indicators;
- failed background/provider operations where observable.

## Definitions

Before a metric enters the scorecard, document:

- stable metric key;
- source;
- period;
- unit;
- numerator/denominator where applicable;
- exclusions;
- known limitations.

Changing a definition creates a measurement discontinuity unless historical values can be recomputed safely and transparently.

## Review

Weekly snapshots provide the evidence layer. Reviews should focus on:

1. data quality and source health;
2. material movement relative to comparable periods;
3. relevant releases or product changes;
4. plausible explanations and competing explanations;
5. the smallest justified follow-up.

A review may conclude that there is insufficient evidence for a change.
