# CI, coverage and security checks

Everything runs on GitHub Actions and is free for a public repository.

## What runs and when

| Workflow / config | Trigger | Purpose |
|---|---|---|
| `.github/workflows/ci.yml` job `build-test` | every push to any branch; pull requests into `main` | Restore, build (Release), run tests with coverage, publish a coverage summary |
| `.github/workflows/ci.yml` job `dependency-audit` | same | Fail on High/Critical NuGet advisories; warn on Low/Moderate |
| `.github/workflows/codeql.yml` | push and PR on `main`; Mondays 06:00 UTC | CodeQL static analysis of the C# source |
| `.github/dependabot.yml` | weekly | Update PRs for NuGet packages and GitHub Actions, grouped by family |
| Secret scanning + push protection | GitHub setting (already enabled) | Blocks pushes that contain credentials |

A push to the same branch cancels the previous run still in progress (`concurrency`).

## Where to see results
- **Checks** section at the bottom of a pull request, or the **Actions** tab.
- **Coverage:** open a run, then the summary page; the HTML report is attached as the `coverage-and-tests` artifact.
- **CodeQL findings:** Security tab, Code scanning.
- **Dependabot PRs:** Pull requests tab.

## Details

**Coverage.** `coverlet.collector` (already referenced by the test project) writes Cobertura files; ReportGenerator turns them into a Markdown summary and an HTML report. Scope: assembly `MyChatBot.Api`, excluding `Data.Migrations.*` and `Program`. There is no minimum threshold yet; the baseline is 40% line / 84% branch, low because endpoints and data access have no tests.

**Dependency audit.** `.github/scripts/check-vulnerable-packages.sh` runs `dotnet list package --vulnerable --include-transitive` (which always exits 0) and parses the severity column. High or Critical fails the job. Current known finding: transitive `MessagePack 2.5.302` (Moderate, GHSA-qhrr-8q5h-9q3h), brought in by the Aspire AppHost; reported as a warning. The compiler reports the same advisory as NU1902.

Run it locally:
```bash
bash .github/scripts/check-vulnerable-packages.sh
```

**CodeQL.** `build-mode: none` analyzes source without building. Needs `security-events: write`, scoped to that job only. The repository's CodeQL "default setup" is not enabled; enabling it would conflict with this workflow.

**Permissions.** Workflows default to `contents: read`.

**Actions are pinned to major versions** (`@v4`, `@v3`); Dependabot proposes updates. Pinning to commit SHAs is stricter and can be adopted later.

## Run the same steps locally

```bash
dotnet restore && dotnet build --configuration Release --no-restore
```
```bash
dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage
```
`coverage/` and `coverage-report/` are git-ignored.

## Not included
- No coverage threshold or required status checks. Add them in branch protection once the checks have proven stable. Do not require pull request reviews if you want to merge your own PRs.
- Tests needing AWS or Docker are not run in CI. When integration tests are added, mark them to skip without credentials.
- SonarCloud / Codacy (extra code-quality analysis) need an account and a token stored as a repository secret.
- Dependabot alerts and security updates are repository settings, separate from `dependabot.yml`.
