# Roadmap

This roadmap keeps near-term repository work explicit without turning speculative ideas into commitments.

## Current Baseline

The repository already contains:

- layered Domain/Application/Infrastructure/API/Web architecture;
- authentication and account management;
- recurring subscription and digital-service management;
- cost summaries and currency conversion;
- savings-plan generation;
- Stripe-backed billing;
- Azure/Terraform infrastructure;
- CI, deployment and infrastructure workflows;
- PL/EN/DE web experience.

## Next Repository-Quality Steps

### Measurement baseline

Status: **Planned**

- fix the common weekly snapshot day/time/timezone policy;
- connect only authoritative aggregate sources;
- create the first validated baseline;
- add the smallest reliable scheduled collection/continuity check;
- preserve missing-data and privacy rules from `docs/measurement-snapshots.md`.

### Operational validation

Status: **Ongoing**

- keep CI, deployment and infrastructure workflows healthy;
- verify production changes with the relevant application and infrastructure checks;
- record reusable deployment failures in `docs/learnings.md`.

### Product-quality follow-up

Status: **Evidence-driven**

Use real product behavior, reliability signals and user-visible issues to choose the next improvements. Prefer small changes with a clear reason and validation path over speculative feature expansion.

## Guardrails

- do not add architecture for hypothetical scale;
- do not add instrumentation without a question it answers;
- do not use roadmap items as promises;
- keep provider-specific concerns at infrastructure boundaries;
- update this file when the actual next repository-level work changes.
