# Testing

Tests should protect meaningful behavior and integration boundaries rather than target a coverage number.

## Priorities

Prefer tests for:

- domain rules and edge cases;
- application use cases with meaningful branching;
- billing and authentication behavior;
- persistence mappings or queries that have regression risk;
- provider adapters where deterministic boundaries can be exercised;
- API behavior where routing, authorization or error contracts matter;
- web behavior where a user-visible regression is plausible.

Avoid tests that merely duplicate framework behavior or assert trivial property assignment.

## Repository Validation

For repository-wide changes:

```powershell
dotnet restore SubscriptionManager.slnx
dotnet build SubscriptionManager.slnx --configuration Release --no-restore
dotnet test SubscriptionManager.slnx --configuration Release --no-build
dotnet format SubscriptionManager.slnx --verify-no-changes
```

Use narrower project/test commands during iteration when they provide faster feedback, then run the relevant repository-level checks before considering a cross-cutting change complete.

## External Services

Tests should not require live Stripe, OpenAI, NBP or email-provider calls unless a deliberately separated integration check is being performed.

Keep provider behavior behind deterministic seams and test repository-owned behavior rather than third-party implementation details.
