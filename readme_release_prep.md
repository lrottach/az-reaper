# Azure Reaper v2.0.0 -- Pre-Release Review Report

## Context

This is a comprehensive pre-release review for the Azure Reaper v2.0.0. Three independent expert reviewers (DevOps, .NET Developer, Security) performed a full audit of the project. This plan consolidates all findings into a unified, prioritized action list for release preparation.

---

## Overall Ratings

| Reviewer | Rating | Summary |
|----------|--------|---------|
| DevOps Expert | **6.0/10** | Strong IaC fundamentals, but no CI/CD or tests -- biggest gaps |
| .NET Developer | **7.5/10** | Clean architecture with a critical scheduling bug and dead code |
| Security Reviewer | **7.5/10** | Security-conscious design; needs infra hardening and safety guards |

---

## CRITICAL Findings (Must Fix Before Release)

### C1. Durable Entity timer scheduling bug -- immediate deletion instead of delayed
- **File:** `src/AzureReaper.Functions/Entities/AzureResourceEntity.cs:113-119`
- **Issue:** `SignalEntityOptions` is passed as the `input` parameter instead of the `options` parameter to `Context.SignalEntity()`. The signal fires immediately (not on schedule) and `DeleteResourceAsync` receives a `SignalEntityOptions` object as input instead of `null`. **Every resource group will be deleted immediately after creation rather than after the configured lifetime.**
- **Fix:** Change to `Context.SignalEntity(Context.Id, nameof(DeleteResourceAsync), input: null, options: signalOptions);`
- **Source:** .NET Developer Review

---

## HIGH Findings (Should Fix Before Release)

### H1. Entity state cleanup does not delete entities from storage
- **File:** `src/AzureReaper.Functions/Entities/AzureResourceEntity.cs:45-46, 52-53, 174-175, 190`
- **Issue:** `State = null!` resets state to default but does NOT remove the entity from durable storage. Leads to unbounded orphaned entity accumulation over time.
- **Fix:** Use `Context.DeleteState()` or equivalent proper entity deletion API.

### H2. Dead code: `StringHandler.cs` and unused `IResourcePayload` interface
- **Files:** `src/AzureReaper.Functions/Common/StringHandler.cs`, `src/AzureReaper.Functions/Interfaces/IResourcePayload.cs`
- **Issue:** `StringHandler.ExtractResourcePayload()` is never called. `IResourcePayload` is implemented by two classes but never used as a parameter type or constraint.
- **Fix:** Remove both files (or integrate `StringHandler` if intended).

### H3. No CI/CD pipelines exist
- **Directory:** `.github/workflows/` (missing)
- **Issue:** No automated builds, tests, linting, Terraform validation, or deployment pipelines. For a tool that deletes resource groups, this is a significant gap.
- **Fix:** Create GitHub Actions workflows for: build+validate (on PR), deploy (on merge to main), release (on tag).

### H4. No unit test project
- **Issue:** The solution contains only the Functions project. Core logic (`TagHandler`, entity state transitions, event filtering) has zero automated test coverage.
- **Fix:** Add a test project with unit tests for at minimum `TagHandler`, `AzureResourceEntity` state transitions, and EventGrid filtering logic.

### H5. Terraform lock file version mismatch
- **File:** `infra/.terraform.lock.hcl:25`
- **Issue:** `time` provider constraint is `~> 0.12` in lock file but `~> 0.13` in `provider.tf`. Will cause `terraform init` failures.
- **Fix:** Run `terraform init -upgrade` to regenerate the lock file.

---

## MEDIUM Findings (Recommended Before Release)

### Infrastructure & DevOps

| # | Finding | File | Fix |
|---|---------|------|-----|
| M1 | Custom role name `"Azure Reaper Operator"` is not unique per environment -- will collide if two envs deploy to same subscription | `infra/function.tf:107` | Append env name: `"Azure Reaper Operator - ${var.AZURE_ENV_NAME}"` |
| M2 | No validation on `AZURE_ENV_NAME` variable -- long names could exceed Azure limits | `infra/variables.tf` | Add length validation |
| M3 | Storage account missing network restrictions (publicly accessible) | `infra/function.tf:8-18` | Add `network_rules` block with `default_action = "Deny"` |
| M4 | Storage account missing explicit `min_tls_version` and `https_traffic_only_enabled` | `infra/function.tf:8-18` | Add explicit settings |
| M5 | Dependabot only covers devcontainers, not NuGet or Terraform | `.github/dependabot.yml` | Add `nuget` and `terraform` ecosystems |
| M6 | Azurite Docker image uses `:latest` tag | `docker-compose.yml:2` | Pin to specific version |

### Code Quality

| # | Finding | File | Fix |
|---|---------|------|-----|
| M7 | `ResourcePayload` uses `required string?` -- contradictory; entity code uses `!` operator extensively assuming non-null | `src/.../Models/ResourcePayload.cs:8-10` | Change to `required string` (non-nullable) |
| M8 | Pervasive null-forgiving `!` on entity state properties -- no guard if state is unexpectedly null | `src/.../Entities/AzureResourceEntity.cs` (7+ locations) | Add null guards or make state properties non-nullable |
| M9 | Unnecessary ASP.NET Core / HTTP packages for a non-HTTP app | `src/.../AzureReaper.csproj:11,21-22` and `Program.cs:12` | Remove HTTP packages, switch `ConfigureFunctionsWebApplication()` to `ConfigureFunctionsWorkerDefaults()` |
| M10 | No `CancellationToken` propagation in async methods | All async methods | Add `CancellationToken` to method signatures and pass to Azure SDK calls |

### Safety & Security

| # | Finding | File | Fix |
|---|---------|------|-----|
| M11 | No resource group exclusion/protection list -- reaper can delete its own RG | Architecture | Add configurable exclusion list; at minimum exclude the reaper's own RG |
| M12 | No minimum lifetime cap -- tag value `1` schedules deletion in 1 minute | `src/.../Common/TagHandler.cs` | Add configurable minimum (e.g., 5 minutes) |
| M13 | Re-entrant event loop from reaper's own tag writes (each tag update triggers EventGrid again) | Architecture | Add EventGrid advanced filtering or short-circuit check before entity signal |

### Documentation

| # | Finding | File | Fix |
|---|---------|------|-----|
| M14 | CLAUDE.md references `infra/main.bicep` -- project uses Terraform now | `.claude/CLAUDE.md:102` | Update to reference Terraform |
| M15 | CLAUDE.md says "Planned: GitHub Actions for CI/CD" -- stale for v2.0.0 | `.claude/CLAUDE.md:128` | Update or remove |
| M16 | README still has "major rewrite" warning banner | `README.md:7-8` | Remove or update for v2.0.0 |
| M17 | DevContainer includes `ms-azuretools.vscode-bicep` extension -- no longer relevant | `.devcontainer/devcontainer.json:37` | Replace with `hashicorp.terraform` |

---

## LOW Findings (Nice to Have)

| # | Finding | File |
|---|---------|------|
| L1 | No `lifecycle` blocks on Terraform resources (e.g., `prevent_destroy` on storage) | `infra/function.tf` |
| L2 | Hardcoded `maximum_instance_count=100` and `instance_memory_in_mb=2048` -- should be variables | `infra/function.tf:77-78` |
| L3 | Log Analytics retention hardcoded to 30 days | `infra/function.tf:33` |
| L4 | No diagnostic settings on storage account or function app | `infra/function.tf` |
| L5 | Inconsistent log prefixes (`[EntityTrigger]` vs class name pattern) | `AzureResourceEntity.cs` |
| L6 | No upper bound on lifetime tag value | `TagHandler.cs` |
| L7 | `launchSettings.json` port 7133 vs documentation/comments referencing 7071 | Multiple files |
| L8 | `DefaultAzureCredential` registered without options -- probes unnecessary credential types | `Program.cs:18` |
| L9 | `archive/` directory should be removed or moved for v2.0.0 release | Repo root |
| L10 | No Terraform backend configured (state stored locally) | `infra/provider.tf` |

---

## Recommended Prioritization for v2.0.0

### Phase 1: Showstoppers (block release)
1. **C1** -- Fix the SignalEntity parameter bug (immediate deletion)
2. **H1** -- Fix entity state cleanup (orphaned entities)
3. **H5** -- Fix Terraform lock file mismatch

### Phase 2: Code Cleanup (high value, low effort)
4. **H2** -- Remove dead code (StringHandler, IResourcePayload)
5. **M9** -- Remove unnecessary ASP.NET Core packages
6. **M7/M8** -- Fix null safety in ResourcePayload and entity
7. **M14-M17** -- Fix stale documentation

### Phase 3: Safety Improvements
8. **M11** -- Add resource group exclusion list (at minimum self-protection)
9. **M12** -- Add minimum lifetime cap
10. **M1** -- Make custom role name environment-unique

### Phase 4: Infrastructure Hardening
11. **M3/M4** -- Storage account network rules and TLS settings
12. **M5/M6** -- Dependabot and Docker image pinning

### Phase 5: CI/CD & Testing (can be post-release but recommended)
13. **H3** -- Create GitHub Actions workflows
14. **H4** -- Add unit test project

---

## Verification

After implementing changes:
1. `dotnet build` succeeds with no warnings
2. `terraform init && terraform validate && terraform plan` succeeds in `infra/`
3. Local testing with `func start` + Azurite -- create a resource group with `CloudReaperLifetime` tag and verify deletion is **scheduled** (not immediate)
4. Verify entity cleanup by checking Azure Table Storage after entity completion
5. Verify tag removal cancels scheduled deletion
6. Review all documentation for accuracy
