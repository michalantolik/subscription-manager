# Measurement Snapshots

Subscription Manager participates in the common weekly measurement snapshot convention used by the project repositories.

The goal is a durable, machine-readable history that can later be compared with releases, commits and engineering decisions.

## Status

**Contract defined — exact shared schedule not activated yet**

The weekly day, exact time, canonical timezone and daylight-saving behavior must be fixed once and then used consistently. Do not introduce a repository-specific schedule.

## Core Contract

Weekly snapshots use:

- the same cadence;
- the same snapshot moment;
- the same seven-day period boundaries;
- the same naming convention;
- the same core metadata;
- repository-specific metrics defined in `docs/measurement.md`.

## Storage

Use a compact structure:

```text
docs/
└── measurement/
    ├── snapshots/
    │   └── 2026/
    │       └── 2026-W37.json
    └── reviews/
        └── 2026/
            └── 2026-W37.md
```

JSON stores facts. Markdown reviews store interpretation only when a review is useful.

## Snapshot Envelope

```json
{
  "schemaVersion": 1,
  "repository": "subscription-manager",
  "snapshotId": "2026-W37",
  "period": {
    "from": "2026-09-07",
    "to": "2026-09-13"
  },
  "capturedAtUtc": "2026-09-14T00:00:00Z",
  "status": "complete",
  "sources": {},
  "metrics": {},
  "changes": [],
  "limitations": []
}
```

The timestamp above is illustrative, not the selected schedule.

## Validation

A snapshot should be rejected or marked incomplete when:

- schema version is unsupported;
- repository or period identity is wrong;
- expected period boundaries do not match;
- a duplicate period exists;
- source status is missing;
- required fields are absent;
- missing data was converted to zero;
- metric semantics changed without being documented.

Continuity checks should also identify a missing previous weekly snapshot.

## Automation

Use progressive automation.

1. Fix the shared schedule and schema.
2. Establish a real baseline from authoritative sources.
3. Add `workflow_dispatch` for controlled testing/recovery.
4. Add scheduled collection only for sources that can be queried reliably.
5. Validate the generated snapshot before preserving it.
6. Fail visibly when collection fails rather than manufacturing data.

GitHub Actions cron uses UTC, so any intended local clock time requires an explicit daylight-saving policy.

## Security and Privacy

- use aggregate evidence;
- never commit credentials or provider secrets;
- use least-privilege authentication for automated collection;
- do not store raw telemetry dumps in Git;
- do not store IP addresses, precise locations, payment details or user-level behavioral histories in snapshots;
- retain only evidence needed for product and technical review.

## Interpretation Guardrails

- one week is an observation, not a trend;
- compare like-for-like periods and definitions;
- preserve denominators;
- account for outages and collection gaps;
- distinguish chronology and correlation from causal evidence;
- do not optimize for a metric merely because it is easy to collect;
- prefer `insufficient evidence` to an unsupported recommendation.

## Review Horizons

- weekly: evidence and data quality;
- monthly: emerging patterns and product changes;
- quarterly: broader product and technical direction;
- 6- and 12-month: long-term trends and sustained outcomes.

Longer reviews should derive from preserved weekly evidence rather than replace it.
