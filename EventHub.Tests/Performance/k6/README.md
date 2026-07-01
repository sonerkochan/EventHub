# EventHub k6 load tests

This folder contains read-only Grafana k6 tests for public event discovery and authenticated Client, Organizer, Supplier, and Admin journeys.

## Prerequisites

Install [Grafana k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) and ensure `k6 version` works from the repository root. The target EventHub environment must be reachable from the machine running k6.

Use dedicated active test accounts for the roles being exercised. Do not put credentials in this repository or in the script.

## Flows

`FLOW` selects the traffic to run:

- `public`: public home page and `/api/Events`; no credentials required.
- `client`: Client login, dashboard, event listing/details, supplier-service catalog, ticket list, and payment history.
- `organizer`: Organizer login, dashboard, event list, and supplier-service catalog.
- `supplier`: Supplier login, dashboard, service list, and rental-request list.
- `auth`: login page and Admin login only.
- `admin`: Admin login, dashboard, and read-only Users, Venues, Rooms, Events, and Tickets pages.
- `all`: public, authentication, and Admin read-only flows, plus any Client, Organizer, and Supplier flows whose credential pairs are supplied.

When `FLOW` is omitted, the script runs `all` if Admin credentials are supplied; otherwise it runs `public`.

The MVC event listing and details routes inherit the Client role requirement in `Areas/Client/Controllers/BaseController.cs`. To include those protected pages, supply `CLIENT_USERNAME` and `CLIENT_PASSWORD`. The script discovers the first published event from `/api/Events`, or uses `EVENT_ID` when supplied. It skips details when no published event exists.

Supplier-service search is implemented by the Client and Organizer service catalogs. Set `SERVICE_SEARCH_TERM` to exercise the existing read-only `searchTerm` query. Event search/filtering and user profile pages are not currently implemented, so the suite does not invent routes for them.

## Load profiles

The default `smoke` profile ramps to 5 virtual users over 1 minute, stays at 5 for 2 minutes, and ramps down for 1 minute.

The `heavy` profile ramps to 20 virtual users over 1 minute, stays at 20 for 2 minutes, and ramps down for 1 minute. Use it after the smoke profile is healthy.

The `stress` profile steps through 10 and 25 virtual users, reaches 50 virtual users for 2 minutes, and ramps down. Use it to identify degradation under steadily increasing load.

The `spike` profile briefly jumps from 5 to 100 virtual users. Use it only to test sudden traffic behavior after explicit approval from the staging owner.

The `soak` profile holds 10 virtual users for 30 minutes. Use it to look for gradual resource exhaustion, connection-pool pressure, or response-time drift.

`stress`, `spike`, and `soak` are opt-in capacity tests. Confirm staging permission, monitoring coverage, and a quiet test window before running them.

## Environment variables

| Variable | Default | Purpose |
| --- | --- | --- |
| `BASE_URL` | `https://staging-eventhub.tryasp.net` | Target application URL |
| `FLOW` | `all` with Admin credentials, otherwise `public` | `public`, `client`, `organizer`, `supplier`, `auth`, `admin`, or `all` |
| `PROFILE` | `smoke` | `smoke`, `heavy`, `stress`, `spike`, or `soak` |
| `ADMIN_USERNAME` | none | Dedicated Admin test username |
| `ADMIN_PASSWORD` | none | Dedicated Admin test password |
| `CLIENT_USERNAME` | none | Dedicated Client test username |
| `CLIENT_PASSWORD` | none | Client test password |
| `ORGANIZER_USERNAME` | none | Dedicated Organizer test username |
| `ORGANIZER_PASSWORD` | none | Organizer test password |
| `SUPPLIER_USERNAME` | none | Dedicated Supplier test username |
| `SUPPLIER_PASSWORD` | none | Supplier test password |
| `EVENT_ID` | discovered | Optional stable published event ID |
| `SERVICE_SEARCH_TERM` | none | Optional supplier-service search term |
| `THINK_TIME_SECONDS` | `1` | Pause between virtual-user iterations |

## Run commands

Public-only smoke test:

```bash
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

All flows in Windows PowerShell:

```powershell
$env:BASE_URL="https://staging-eventhub.tryasp.net"
$env:ADMIN_USERNAME="testadmin"
$env:ADMIN_PASSWORD="your-password"
$env:FLOW="all"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Client read-only journey in PowerShell:

```powershell
$env:FLOW="client"
$env:CLIENT_USERNAME="testclient"
$env:CLIENT_PASSWORD="your-password"
$env:SERVICE_SEARCH_TERM="catering"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Organizer read-only journey in PowerShell:

```powershell
$env:FLOW="organizer"
$env:ORGANIZER_USERNAME="testorganizer"
$env:ORGANIZER_PASSWORD="your-password"
$env:SERVICE_SEARCH_TERM="sound"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Supplier read-only journey in PowerShell:

```powershell
$env:FLOW="supplier"
$env:SUPPLIER_USERNAME="testsupplier"
$env:SUPPLIER_PASSWORD="your-password"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Heavier profile:

```powershell
$env:PROFILE="heavy"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Stress profile, after staging approval:

```powershell
$env:PROFILE="stress"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Spike profile, only during an approved test window:

```powershell
$env:PROFILE="spike"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

Soak profile:

```powershell
$env:PROFILE="soak"
k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
```

## Authentication assumptions

The login helper first requests `/User/Login`, extracts the MVC `__RequestVerificationToken`, and posts `Username`, `Password`, and the token back to `/User/Login`. k6 keeps the resulting Identity cookies in the virtual user's cookie jar. The script follows redirects and verifies that the login form is no longer rendered. Each role flow then verifies its expected area and page content.

Requests ask for English content so text checks remain stable. No secrets are logged or stored.

## Thresholds

The suite requires fewer than 1% failed HTTP requests, more than 95% successful checks, fewer than 1% failed flow checks, and p95 response times below 2 seconds for public and role-specific pages. Authentication has a 2.5-second p95 threshold. Custom trends report Client, Organizer, Supplier, Admin, and supplier-service catalog timings separately.

These tests intentionally cover only read-only traffic. Never extend a shared staging load run with ticket purchases, service rentals, ticket refunds, application approval, user changes, event publishing/cancellation, seat-layout changes, or other persistent mutations.
