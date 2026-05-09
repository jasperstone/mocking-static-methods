### 2026-05-07: Re-include Orleans BVT category in coverage filter
**By:** Beck (Test & Coverage Engineer), requested by Jasper
**Context:** Test-discovery diagnostic showed Orleans `ServiceBus.Tests` had 108
universe tests but only 14 passing the FILTER (87% excluded). Overall Orleans
coverage was 6.07% — lowest of all 7 repos. Investigation traced this to four
Orleans-specific category exclusions added during the unit-only filter pass on
2026-05-01: `Category!=BVT&Category!=SlowBVT&Category!=LoadShedding&Category!=CorePerf`.

**Category inventory (Orleans pinned SHA `8024faf8`):**

| Category | TestCategory attr uses | Files | Verdict | Sample evidence |
|---|---|---|---|---|
| **BVT** | 54 (26 `[TestCategory]` + 28 `[Trait]`) attribute occurrences across **159 files** | many | **RE-INCLUDE** | `test/Extensions/ServiceBus.Tests/EvictionStrategyTests/EHPurgeLogicTests.cs` — pure mocks (`CachePressureInjectionMonitor`, `PurgeDecisionInjectionPredicate`, no silo, no network). Classic unit test. Orleans tradition: BVT = "Build Verification" = broad correctness suite, predominantly unit-level. |
| **SlowBVT** | 40 occurrences | ~5+ | **KEEP EXCLUDING** | `test/Tester/HeterogeneousSilosTests/UpgradeTests/UpgradeTests.cs`, `ClientConnectionEventTests.cs` — heterogeneous-silo upgrade & client-connection scenarios. Integration-flavored, slow. |
| **LoadShedding** | 2 occurrences | 1 | **KEEP EXCLUDING** | `test/TesterInternal/General/LoadSheddingTest.cs` — stress test by name and intent. |
| **CorePerf** | 6 occurrences | 2 | **KEEP EXCLUDING** | `test/TesterInternal/StorageTests/PersistenceGrainTests.cs`, `GrainPersistenceTestRunner.cs` — performance benchmarks. |

**Cross-check:** Orleans defines `[TestCategory(string)]` in
`test/TestInfrastructure/TestExtensions/TestCategory.cs` which emits a
`Category=<name>` xunit trait via `CategoryDiscoverer`. So `Category!=BVT` in the
dotnet test filter does match `[TestCategory("BVT")]` attributes — exclusion was
working as intended, just over-broad.

**Project-discovery glob gap:** The diagnostic's `find test -name "*.Tests.csproj"`
glob misses `test/Orleans.Serialization.UnitTests/Orleans.Serialization.UnitTests.csproj`
and `test/Orleans.Dashboard.Tests/Orleans.Dashboard.UnitTests/Orleans.Dashboard.UnitTests.csproj`
because they end in `.UnitTests.csproj`. The coverage workflow itself runs
`dotnet test Orleans.slnx` so those projects DID run during coverage — the gap
was only in the per-project diagnostic CSV. Fixed by extending glob to
`\( -name "*.Tests.csproj" -o -name "*.UnitTests.csproj" \)`. Tester.X.csproj
projects (`Tester.AdoNet`, `Tester.AzureUtils`, `Tester.Cassandra`, `Tester.Cosmos`,
`Tester.Redis`, `Tester.ZooKeeperUtils`) deliberately not added — they spin up
real infrastructure and are integration-class.

**Files changed:**
- `.github/workflows/coverage-orchestrator.yml` — orleans job's test step: removed `&Category!=BVT` from `--filter`. Added comment explaining decision and date.
- `.github/workflows/test-discovery.yml` — orleans job's list step: same FILTER change, plus glob expansion to include `*.UnitTests.csproj`.

**Both filters still match exactly** (BVT removed from both; SlowBVT/LoadShedding/CorePerf retained in both).

**Expected impact:** Orleans coverage should rise substantially from 6.07%; the
ServiceBus.Tests 14/108 pass count should grow to include the BVT-tagged
EventHub eviction/streaming unit tests. Some BVT tests carry secondary categories
that need external infrastructure (`AzureStorage`, `EventHub`, `Cosmos`); those
will fail/skip in CI but won't gate the run since the coverage job uses
`continue-on-error: true`.

**Not changed:** `Category!=Functional` remains in the standard exclusion bundle.
Orleans does use `[TestCategory("Functional")]` heavily (171 files), but the
project-glob already excludes path-based `*FunctionalTests*` projects and the
`Functional` trait in Orleans typically marks end-to-end behavior tests. Holding
that line for now; revisit if coverage gain from BVT alone is insufficient.

**Workflow not triggered.** Jasper will dispatch coverage with `repo=all`.
