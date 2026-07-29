# 💎 FWA Stats

Web site that collects and publishes clan war statistics for [FWA – Farm War Alliance](https://www.farmwaralliance.org/), an alliance of [Clash of Clans](https://clashofclans.com/) clans that sync their clan wars with each other.

Live site: [fwastats.com](https://fwastats.com)

The clan list is read from the Proud Clans of FWA Google Sheet. Clan, player and war details are fetched from the [Clash of Clans API](https://developer.clashofclans.com/) and stored in a local database, which is refreshed roughly every 15 minutes. Weighing results are read from the [FWA - Weights Summary](https://docs.google.com/spreadsheets/d/1puJaarYW_nhRpTeTnUBzdy2nAkFZ32OgWSwFkWAaHjU/edit?gid=0#gid=0) sheet.

> **Note:** the Clash API does not expose war start times, only end times. To prevent the site from being used for sync stalking, current war details are hidden until ~2 hours after the war search (`Constants.HIDE_TIME`).

## Features

- **Clans** – clan list with war weights, town hall distribution, war log, war details and attacks, member list and events (joins, leaves, promotions, name changes, TH upgrades)
- **Following clans** – clans that have synced with the alliance but are not on the official clan list, plus recently departed clans
- **Syncs** – war sync windows read from a Google Calendar (or a [band.us](https://band.us/) iCal feed), matched against actual wars to show which clans synced and which mismatched
- **Players** – player search, player history and linking a player tag to a site account
- **Weights** – war weight entry and submission back to the FWA weight sheets, with per-clan submit access control and submit limits
- **Data API** – clan, war, member and weight data as JSON, XML or CSV (see `/Data`)
- **Accounts** – ASP.NET Core Identity with local accounts, optional Google login, email confirmation and two-factor support

Most pages are behind a login wall; `Clans`, `Players`, `Syncs` and the `Data` index require an authenticated user.

## Tech stack

| Area | Choice |
| --- | --- |
| Runtime | .NET 10 (`net10.0`), ASP.NET Core MVC + Razor Pages |
| SDK | pinned in `global.json` (10.0.302, `rollForward: latestFeature`) |
| Data | Entity Framework Core 10 – SQLite (default) or SQL Server |
| Auth | ASP.NET Core Identity, optional Google external login |
| Logging | NLog (`NLog.config`) |
| Front end | Bootstrap 5, Font Awesome, tablesort – restored with LibMan (`libman.json`), bundled/minified by WebOptimizer |
| External services | Clash of Clans API, Google Sheets & Calendar, Firebase Realtime Database, Google Apps Script (weight submit), SMTP |
| JSON | Newtonsoft.Json (see the note in `FWAStatsWeb.csproj` for why it is not System.Text.Json) |
| CI/CD | Azure Pipelines (`src/FWAStatsWeb/azure-pipelines/`) |

## Repository layout

```
FWAStatsWeb.slnx              Solution (slnx format)
global.json                   Pinned .NET SDK version
src/FWAStatsWeb/
  Program.cs                  Host, DI, middleware and options wiring
  Constants.cs                War sizes, cache keys, max weights per TH level
  Controllers/                Home, Clans, Players, Syncs, Data, Update, Account, Manage
  Logic/                      Clash API client, clan loader/updater, statistics, weight calculator, Google services
  Services/                   Weight submit queue + hosted worker, e-mail senders
  Models/                     EF entities and view models
  Data/                       ApplicationDbContext and EF Core migrations
  Formatters/                 CSV input/output formatters for the data API
  Views/                      Razor views
  wwwroot/                    Static assets (client libraries are committed)
  azure-pipelines/            Build and deploy pipelines
```

## Getting started

### Prerequisites

- .NET SDK 10.0.302 or newer (see `global.json`)
- An editor – [Visual Studio Code](https://code.visualstudio.com/) or a current [Visual Studio](https://visualstudio.microsoft.com/) both work; the solution is in the `slnx` format
- A [Clash of Clans API token](https://developer.clashofclans.com/) tied to the IP the app runs from
- Optional, for the Google-backed features: a Google service account (Sheets + Calendar), a Firebase Realtime Database URL and the weight submit script URLs

### Configuration

`appsettings.json` holds the non-secret defaults (clan list sheets, weight database, result sheets, submit URLs). Everything else – connection strings, the Clash API token, Google credentials, SMTP – comes from `appsettings.Sample.json`, which is the template for environment-specific configuration.

For local development, put the secrets in user secrets rather than editing tracked files (the project already has a `UserSecretsId`):

```powershell
cd src/FWAStatsWeb
dotnet user-secrets set "ConnectionStrings:Default" "SQLite"
dotnet user-secrets set "ConnectionStrings:SQLite" "Data Source=ClashDB.sqlite"
dotnet user-secrets set "ClashApi:Token" "<your token>"
dotnet user-secrets set "KeyStorageFolder" "keys"
```

Key settings:

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:Default` | Selects the provider – `SQLite` or `SqlServer`; the value names the connection string to use |
| `ClashApi:Url` / `Token` | Clash of Clans API endpoint and bearer token |
| `ClanLists` | Google Sheet id/range for the FWA clan list and the blacklist |
| `Statistics:Wars` | How many days of war history to keep |
| `Statistics:CalendarId` / `SyncURL` | Google Calendar id, or a band.us iCal feed, for war sync times |
| `WeightDatabase` | Firebase database + weight sheet used to import member war weights |
| `ResultDatabase` | Heat map sheets per team size in [FWA - Weights Summary](https://docs.google.com/spreadsheets/d/1puJaarYW_nhRpTeTnUBzdy2nAkFZ32OgWSwFkWAaHjU/edit?gid=0#gid=0), used for weight submit results |
| `WeightSubmit` | Google Apps Script endpoints that receive submitted weights |
| `GoogleService` | Service account e-mail and private key for Sheets/Calendar access |
| `Smtp` | Outgoing mail for account confirmation and password reset |
| `Authentication:Google` | Enable and configure Google external login |
| `KeyStorageFolder` | Folder for Data Protection keys (required – startup fails if missing) |

### Run

```powershell
dotnet run --project src/FWAStatsWeb
```

The app listens on http://localhost:5000 in the `FWAStatsWeb` launch profile. Optionally restore the client libraries with `libman restore` – they are committed, so this is only needed when `libman.json` changes.

Database migrations are applied automatically when `/Update` is opened, so browse there first on an empty database.

## Keeping the data fresh

There is no in-process scheduler for the Clash data. The web app only exposes the update steps as endpoints under `/Update`, and something outside the app has to drive them:

1. `/Update/GetTasks` reads the clan list from Google Sheets, diffs it against the database and returns insert/update/delete tasks.
2. Each task is executed with `/Update/UpdateTask/{id}`; these can run in parallel.
3. When the queue drains, `/Update/UpdateFinished` runs the follow-up work: history cleanup, clan validities, sync calculation and matching, clan stats, blacklist refresh, weight import from Firebase and weight result import from the sheets. A full weight re-import runs on Mondays just after midnight UTC.
4. `/Update/PlayerBatch` returns the next batch of player tags (200 at a time), each refreshed with `/Update/UpdatePlayerTask/{tag}`.

In production this is done by **[FWAStatsJob](https://dev.azure.com/ptim74/FWAStats/_git/FWAStatsJob)**, a separate console application that is installed on the server and run on a schedule (about every 15 minutes). It walks exactly the sequence above with 8 threads, retries failed clan updates once, and aborts the queue when the Clash API starts returning protocol errors.

The same flow is available interactively: opening `/Update` in a browser shows the pending tasks and the **Confirm** button runs them with 8 parallel requests from the page, and `/Update/Players` does the same for players. Individual follow-up steps can be triggered on their own via `/Update/Task/{name}` (`deletehistory`, `updatevalidities`, `calculatesyncs`, `updatesyncmatch`, `updateclanstats`, `blacklisted`, `weights`, `allweights`, `updateresults`).

Weight submissions are handled separately: `WeightSubmitService` queues them and `HostedWebSubmitService`, a `BackgroundService`, drains the queue once per second and posts to the Google Apps Script endpoint.

## Data API

Available as `json`, `xml` or `csv` by changing the extension:

| Endpoint | Description |
| --- | --- |
| `/Clans.{format}` | All clans with level, points, war record, TH counts and estimated weight |
| `/Clan/{tag}/Members.{format}` | Clan members with TH level, role, donations and weight |
| `/Clan/{tag}/WarMembers.{format}?warNo=1` | War roster and attacks (`warNo=1` is the latest war) |
| `/Clan/{tag}/Wars.{format}` | War history with result, sync offset and opponent classification |
| `/Weights.{format}` | All known member weights |
| `/WeightUpdates.{format}` | Weights changed on the site in the last 28 days |

`{tag}` is the clan tag without the `#`. `/Data` documents these with live samples and shows how to pull them into Google Sheets or Excel.

## Logging

NLog writes to `logs/` next to the binaries: `error.log` (all errors, daily archive), `update.log` (`UpdateController` and `ClanStatistics`, hourly archive) and `info.log` (everything else). Microsoft and System logs below error level are dropped.

## Deployment

Azure Pipelines definitions live in `src/FWAStatsWeb/azure-pipelines/`:

- `template.yml` – build, publish and deploy to a Linux VM environment, then restart the `fwastats.service` systemd unit and smoke-test `/update`
- `template-webapp.yml` – build and zip-deploy to an Azure Web App
- `thor.yml`, `north.yml` – production deployments (triggered from `master`)
- `demo.yml`, `test.yml` – manually triggered Azure Web App environments

Both templates copy `appsettings.Sample.json` to `appsettings.Production.json` and let the `FileTransform` task substitute the real values from pipeline variable groups.

The production site runs on Ubuntu Linux behind a reverse proxy (`UseForwardedHeaders` is enabled).

## Related projects

- **[FWAStatsJob](https://dev.azure.com/ptim74/FWAStats/_git/FWAStatsJob)** – console application that drives the `/Update` endpoints on a schedule so the database refresh does not have to be started from a browser. It has its own Azure Pipelines definitions and is deployed next to the site (`~/fwastatsjob`); on Azure Web Apps it runs as a WebJob via `run.cmd`, which points it at `%WEBSITE_HOSTNAME%`.

## History

The site started as a hobby project for learning Azure and ASP.NET web development, was ported to .NET Core 2.0, and has been kept on the current .NET release since.

--
War Farmers 18 | pete
