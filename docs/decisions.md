# Engineering Decisions

This file records durable decisions that help future work understand why the repository is structured the way it is. It is not a changelog.

## Layered architecture with feature-oriented projects

**Decision:** Keep Domain, Application, Infrastructure, API and Web as explicit project boundaries, while organizing code by product feature within those projects.

**Reason:** The dependency direction keeps business rules independent of delivery and provider details while feature-oriented folders make related behavior easy to locate.

## Web communicates with the API over HTTP

**Decision:** Keep the Blazor Web project as an API client rather than directly using application or persistence services.

**Reason:** This preserves a clear application boundary and keeps the web host aligned with the deployed architecture.

## Provider integrations remain in Infrastructure

**Decision:** Keep Stripe, NBP, email delivery, persistence and OpenAI implementation details in Infrastructure behind application-owned contracts where appropriate.

**Reason:** External providers can change independently of the core use cases and domain model.

## Infrastructure is managed as code

**Decision:** Keep Azure infrastructure under `infra/` and validate/deploy it through the existing infrastructure workflow.

**Reason:** Versioned infrastructure makes environment changes reviewable and repeatable.

## Weekly measurement uses durable aggregate snapshots

**Decision:** Preserve a small weekly machine-readable measurement checkpoint with stable definitions and explicit source status.

**Reason:** Historical snapshots make product and technical changes easier to evaluate over time without storing raw telemetry in Git.

**Consequence:** Snapshot facts remain separate from interpretation. Missing data is explicit, metric definitions are versioned when semantics change, and the exact shared schedule is activated only after its day/time/timezone policy is fixed.
