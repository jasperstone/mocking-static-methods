# mocking-static-methods
Experiment in generating unit tests and mocks for code containing static method calls

## Setup

### Option 1: Dev Container (Recommended)

The easiest way to get started is using the included dev container, which provides all dependencies pre-configured:

1. **Prerequisites:**
   - [Docker Desktop](https://www.docker.com/products/docker-desktop)
   - [VS Code](https://code.visualstudio.com/)
   - [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

2. **Open in container:**
   - Open this folder in VS Code
   - Click "Reopen in Container" when prompted (or use Command Palette: "Dev Containers: Reopen in Container")
   - Wait for container to build and install dependencies

3. **Set up your GitHub token:**
   - Create a `.env` file: `cp .env.example .env`
   - Add your GitHub personal access token
   - Get a token from: https://github.com/settings/tokens

4. **You're ready!** All tools (.NET SDKs, Python, PowerShell, coverage tools) are pre-installed.

### Option 2: Local Installation

If you prefer to run locally without containers:

### Prerequisites
- .NET 8.0 SDK
- Python 3.8+
- GitHub Personal Access Token

### Configuration

1. **Install Python dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Set up your GitHub token:**
   - Copy the `.env` file and add your GitHub personal access token
   - Get a token from: https://github.com/settings/tokens
   - Edit `.env` and replace `your_github_token_here` with your actual token
   
   ```bash
   # Example .env content:
   GITHUB_TOKEN=ghp_your_actual_token_here
   NUM_REPOS=2
   ```

3. **Build the C# analyzer:**
   ```bash
   dotnet build StaticCallAnalyzer/StaticCallAnalyzer.csproj
   ```

### Running

#### Command Line
```bash
python orchestrator.py
```

#### Debugging in VS Code
- Use the "Debug Orchestrator" configuration for Python debugging
- Use the "Debug StaticCallAnalyzer" configuration for C# debugging
- Set breakpoints as needed

### Security Note
- The `.env` file is gitignored to keep your GitHub token secure
- Never commit your actual token to version control

## Cloned Repositories

### Algorithm & Goals

**Objective:** Generate comprehensive unit tests and mocks for code containing static method calls across multiple open-source .NET repositories.

**Build & Test Pseudocode:**
```bash
# Generic pseudocode for each repository
cd cloned_repos/<repository_name>
git checkout <pinned_release>
<restore_command>          # dotnet restore, ./restore.sh, ./autogen.sh, etc.
<build_command>            # dotnet build, make, ./build.sh, etc.
<test_with_coverage>       # dotnet test --collect:"XPlat Code Coverage", make check && make coverage, etc.
echo "Coverage data: cloned_repos/<repository_name>/<coverage_path>/"
```

**Example Implementation:**
```bash
cd cloned_repos/efcore && git checkout rel10.1 && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data: cloned_repos/efcore/TestResults/"
```

Each repository below implements this algorithm with project-specific commands.

### Repository Build & Test Scripts

The following table tracks the repositories cloned into the `cloned_repos/` directory with complete build and test scripts:

| Description | GitHub Link | .NET Version | Build & Test Script |
|-------------|-------------|--------------|---------------------|
| App Framework | [https://github.com/abpframework/abp](https://github.com/abpframework/abp) | .NET 10 | [See below](#abp-framework-build--test-command) |
| Web Framework | [https://github.com/dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | SDK 10.0.101 | [See below](#aspnet-core-build--test-command) |
| ORM | [https://github.com/dotnet/efcore](https://github.com/dotnet/efcore) | SDK 10.0.102 | [See below](#ef-core-build--test-command) |
| XPlat Runtime | [https://github.com/mono/mono](https://github.com/mono/mono) | Native (autotools) | **Skipped** - Final release Feb 2024, archived project |
| Distributed Actors | [https://github.com/dotnet/orleans](https://github.com/dotnet/orleans) | .NET 10 | [See below](#orleans-build--test-command) |
| Compiler | [https://github.com/dotnet/roslyn](https://github.com/dotnet/roslyn) | SDK 10.0.102 | [See below](#roslyn-build--test-command) |
| .NET Runtime | [https://github.com/dotnet/runtime](https://github.com/dotnet/runtime) | SDK 10.0.100 | [See below](#runtime-build--test-command) |
| AI SDK | [https://github.com/microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) | SDK 10.0.100 | [See below](#semantic-kernel-build--test-command) |
| Auth Server | [https://github.com/DuendeArchive/IdentityServer4](https://github.com/DuendeArchive/IdentityServer4) | .NET 5/6 | **Skipped** - Archived, moved to Duende IdentityServer (commercial) |
| Subtitle Editor | [https://github.com/SubtitleEdit/subtitleedit](https://github.com/SubtitleEdit/subtitleedit) | Framework 4.8 | **Skipped** - Uses .NET Framework 4.8, no .NET 10 support |

> **Note:** Build commands may vary based on the specific repository structure and requirements. Some repositories may require additional setup steps or have custom build scripts. Always check the repository's README for specific build instructions.

### Phase 2 Expansion (May 2026)

To grow the sample set from 7 to a more meaningful 13–15 repositories, we ran a Roslyn-semantic Mode #1 detector ([Mode1Analyzer/](Mode1Analyzer/)) over candidate codebases discovered via GitHub Code Search ([tools/repo_search/](tools/repo_search/)). Repositories were selected for the coverage CI matrix using the following criteria:

1. **Linux-buildable in a containerized .NET SDK** (`mcr.microsoft.com/dotnet/sdk:10.0-noble`). Anything requiring Windows-only UI frameworks (WinUI, WPF) is out.
2. **Active project** — not deprecated, not archived.
3. **Measurable Mode #1 footprint** — enough call sites to a non-mockable static API surface (`ILogger`/`HttpClient`/`IConfiguration`/`IServiceProvider`) to materially affect coverage if a developer attempted to write tests against them.
4. **Reasonable CI cost** — not requiring multi-GB platform SDKs (Android, iOS, Mac) on top of the .NET SDK.

| Repository | GitHub Link | Mode #1 sites | Status | Rationale |
|------------|-------------|---------------|--------|-----------|
| jellyfin | [jellyfin/jellyfin](https://github.com/jellyfin/jellyfin) | 1,206 | ✅ Included | Cross-platform .NET media server, clean `dotnet test` flow |
| garnet | [microsoft/garnet](https://github.com/microsoft/garnet) | 745 | ✅ Included | Microsoft Redis-like server, well-tested |
| server (Bitwarden) | [bitwarden/server](https://github.com/bitwarden/server) | 181 | ✅ Included | Cross-platform .NET; `RustSdk.csproj` excluded by per-csproj test build (no Rust toolchain in container) |
| eShop | [dotnet/eShop](https://github.com/dotnet/eShop) | 94 | ✅ Included | Microsoft reference architecture; `tests/ClientApp.UnitTests` excluded (requires `maui-tizen` workload) |
| duplicati | [duplicati/duplicati](https://github.com/duplicati/duplicati) | 34 | ✅ Included | Cross-platform backup tool, standard .NET test layout |
| Avalonia | [AvaloniaUI/Avalonia](https://github.com/AvaloniaUI/Avalonia) | 9 | ✅ Included | Cross-platform UI; restore scoped to `tests/*.UnitTests.csproj` to avoid Android/iOS/wasm sample projects |
| MAUI | [dotnet/maui](https://github.com/dotnet/maui) | 329 | ⚠️ Deferred | In-tree build resolves projects targeting `net10.0-android36.0`, requiring the **Android SDK** (~3-5 GB) on top of the `maui-android` workload. Three rounds of remediation each uncovered another layer of MS-internal-CI assumption. Deferred — job body retained in the workflow but the `if:` gate keeps it dormant unless explicitly dispatched |
| Files | [files-community/Files](https://github.com/files-community/Files) | 163 | ❌ Excluded | WinUI3 / Windows-only — cannot build in Linux container |
| PowerToys | [microsoft/PowerToys](https://github.com/microsoft/PowerToys) | 66 | ❌ Excluded | WPF / Windows-only — cannot build in Linux container |
| OpenRA | [OpenRA/OpenRA](https://github.com/OpenRA/OpenRA) | 13 | ❌ Excluded | Mode #1 footprint too low to affect coverage signal |
| StockSharp | [StockSharp/StockSharp](https://github.com/StockSharp/StockSharp) | 3 | ❌ Excluded | Mode #1 footprint too low to affect coverage signal |

**Result:** 13 repos in the active matrix (7 original + 6 added), covering ~6,500 of 6,879 detected Mode #1 sites (94%).

### Containerized Builds

For projects with complex dependencies, you can use Docker to ensure a consistent build environment:

```bash
# Example: Build ABP in a container
docker run --rm -v "$(pwd)/cloned_repos/abp:/workspace" -w /workspace/framework \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore && dotnet build && dotnet test --collect:'XPlat Code Coverage' --results-directory ./TestResults"
```

**Benefits:**
- ✅ Isolated dependencies per project
- ✅ Reproducible builds across machines
- ✅ No conflicts with local environment
- ✅ Easily switch between .NET versions

To create containerized build scripts for each repository, you can:
1. Create a `Dockerfile` in each `cloned_repos/<repo>/` directory
2. Use Docker Compose to orchestrate multiple builds
3. Or use the dev container approach for the entire workspace

#### ABP Framework Build & Test Command

```bash
cd cloned_repos/abp && \
git checkout 10.0.2 && \
cd framework && \
dotnet restore && \
dotnet build && \
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/abp/framework/CoverageReport/index.html"
```

#### ASP.NET Core Build & Test Command

```bash
cd cloned_repos/aspnetcore && \
# Commit ecb199c2 from release/10.0 (Jan 6, 2026) - SDK 10.0.101
# Note: Cannot use tags (v10.0.0, v10.0.2) as they reference internal RC/servicing builds
git checkout ecb199c29cbefb6fcb6aa789436de36e44427a78 && \
git submodule update --init --recursive && \
source ./activate.sh && \
find src -name "*.Tests.csproj" -o -name "*FunctionalTests.csproj" | while read proj; do dotnet add "$proj" package coverlet.collector 2>&1 | grep -q "added" || true; done && \
dotnet test AspNetCore.slnx \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/aspnetcore/CoverageReport/index.html"
```

**Note**: This specific commit snapshot uses a publicly-available released SDK version (10.0.101), avoiding RC or servicing builds not available in public feeds. Tags like `v10.0.0` and `v10.0.2` fail because they reference internal Microsoft builds. The command tests the full AspNetCore.slnx solution with 137+ test projects, adding coverlet.collector to all test projects for comprehensive coverage collection. The `dotnet test` command automatically restores and builds before testing. This provides coverage across all ASP.NET Core components including MVC, Razor, SignalR, Identity, Middleware, Kestrel, and more. Note: Adding coverlet to 137 projects may take 15-30 minutes, followed by build and test time.

#### EF Core Build & Test Command

```bash
cd cloned_repos/efcore && \
git checkout release/10.0 && \
source ./activate.sh && \
dotnet test EFCore.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/efcore/CoverageReport/index.html"
```

**Note**: EF Core on branch `release/10.0` uses the local SDK via `activate.sh` (version 10.0.102). Successfully tested EFCore.sln with 49,056 tests passed across multiple test projects (includes EFCore.Tests with 6,622 tests, Sqlite.FunctionalTests with 37,278 tests, and 12 other test projects). The `dotnet test` command automatically builds before testing.

#### Orleans Build & Test Command

```bash
cd cloned_repos/orleans && \
git checkout v10.0.0 && \
dotnet restore Orleans.slnx && \
dotnet test Orleans.slnx \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/orleans/CoverageReport/index.html"
```

**Note**: Orleans v10.0.0 uses .NET 10. Orleans is Microsoft's framework for building distributed applications using the virtual actor model with features like stateful grains, streams, and transactions.

#### Roslyn Build & Test Command

```bash
cd cloned_repos/roslyn && \
git checkout release/dev18.3 && \
dotnet test Roslyn.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/roslyn/CoverageReport/index.html"
```

**Note**: Roslyn on the **release/dev18.3** branch requires .NET 10.0.100-rc.2 but works with .NET 10.0.102 stable via rollForward policy. This is a development branch tied to Visual Studio 2025 Preview. Roslyn is the .NET Compiler Platform providing C# and Visual Basic compilers with rich code analysis APIs. The `dotnet test` command automatically restores and builds before testing.

#### Runtime Build & Test Command

```bash
cd cloned_repos/runtime && \
git checkout v10.0.2 && \
./build.sh -subset libs+libs.tests -test && \
reportgenerator -reports:"./artifacts/TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/runtime/CoverageReport/index.html"
```

**Note**: .NET Runtime v10.0.2 (released Dec 11, 2025) is the latest patch release for .NET 10. This command builds and tests only the libraries (`libs+libs.tests`) without building the CoreCLR native runtime, which requires clang 8-22 and C++ build tools. The libraries subset includes all managed .NET Base Class Libraries (BCL) like System.Collections, System.IO, System.Text.Json, etc. The `- test` flag runs all library unit tests after building. The .NET Runtime repository is the largest .NET codebase containing the fundamental runtime, base class libraries, and host components. **Prerequisites**: To build the full CoreCLR runtime (not just libraries), install: `sudo apt install clang cmake build-essential`. Library tests run against the pre-installed .NET 10.0.102 SDK and don't require native compilation. Test results are placed in `artifacts/TestResults/` with thousands of test assemblies.

#### Semantic Kernel Build & Test Command

```bash
cd cloned_repos/semantic-kernel && \
git checkout dotnet-1.70.0 && \
cd dotnet && \
dotnet test SK-dotnet.slnx \
  --filter 'FullyQualifiedName!~SemanticKernel.IntegrationTests' \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/semantic-kernel/dotnet/CoverageReport/index.html"
```

**Note**: Semantic Kernel v1.70.0 uses **.NET 10.0.100 SDK** (upgraded from .NET 9 in v1.68.0). The SK-dotnet.slnx solution includes the core Semantic Kernel library and its connectors for OpenAI, Azure OpenAI, Gemini, and other AI services, along with comprehensive test coverage. Semantic Kernel is Microsoft's SDK for integrating large language models (LLMs) into .NET applications with features like prompt templating, function calling, memory, and agents. The `dotnet test` command automatically restores and builds before testing. The filter excludes integration tests (namespace-based: `SemanticKernel.IntegrationTests`) that require API keys for OpenAI, Azure OpenAI, and other services.

### Coverage Report Generation

After running tests with coverage collection, you can generate HTML reports using:

```bash
# Install the reportgenerator tool (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html
```
