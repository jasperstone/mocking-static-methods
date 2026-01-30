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

| Repository | GitHub Link | Build & Test Script | Engineer |
|------------|-------------|---------------------|----------|
| abp | [https://github.com/abpframework/abp](https://github.com/abpframework/abp) | [See below](#abp-framework-build--test-command) | jasper |
| aspnetcore | [https://github.com/dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | [See below](#aspnet-core-build--test-command) | jasper |
| efcore | [https://github.com/dotnet/efcore](https://github.com/dotnet/efcore) | [See below](#ef-core-build--test-command) | jasper |
| mono | [https://github.com/mono/mono](https://github.com/mono/mono) | **Skipped** - Final release Feb 2024, archived project | - |
| orleans | [https://github.com/dotnet/orleans](https://github.com/dotnet/orleans) | [See below](#orleans-build--test-command) | jasper |
| roslyn | [https://github.com/dotnet/roslyn](https://github.com/dotnet/roslyn) | [See below](#roslyn-build--test-command) | jasper |
| runtime | [https://github.com/dotnet/runtime](https://github.com/dotnet/runtime) | | q |
| semantic-kernel | [https://github.com/microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) | [See below](#semantic-kernel-build--test-command) | jasper |
| server | [https://github.com/IdentityServer/IdentityServer4](https://github.com/IdentityServer/IdentityServer4) | | |
| subtitleedit | [https://github.com/SubtitleEdit/subtitleedit](https://github.com/SubtitleEdit/subtitleedit) | | |

> **Note:** Build commands may vary based on the specific repository structure and requirements. Some repositories may require additional setup steps or have custom build scripts. Always check the repository's README for specific build instructions.

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
echo "Coverage data: cloned_repos/abp/framework/TestResults/"
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
echo "Coverage data: cloned_repos/aspnetcore/TestResults/"
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
echo "Coverage data: cloned_repos/efcore/TestResults/"
```

**Note**: EF Core on branch `release/10.0` uses the local SDK via `activate.sh` (version 10.0.102). Successfully tested EFCore.sln with 49,056 tests passed across multiple test projects (includes EFCore.Tests with 6,622 tests, Sqlite.FunctionalTests with 37,278 tests, and 12 other test projects). The `dotnet test` command automatically builds before testing.

#### Orleans Build & Test Command

```bash
cd cloned_repos/orleans && \
git checkout v9.2.1 && \
dotnet restore Orleans.sln && \
dotnet test Orleans.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
echo "Coverage data: cloned_repos/orleans/TestResults/"
```

**Note**: Orleans v9.2.1 uses .NET 8 (installed via devcontainer feature). Successfully tested Orleans.sln with 309 tests passed across 20 test projects. Orleans is Microsoft's framework for building distributed applications using the virtual actor model.

#### Roslyn Build & Test Command

```bash
cd cloned_repos/roslyn && \
git checkout release/dev18.3 && \
dotnet test Roslyn.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
echo "Coverage data: cloned_repos/roslyn/TestResults/"
```

**Note**: Roslyn on the **release/dev18.3** branch requires .NET 10.0.100-rc.2 but works with .NET 10.0.102 stable via rollForward policy. This is a development branch tied to Visual Studio 2025 Preview. Roslyn is the .NET Compiler Platform providing C# and Visual Basic compilers with rich code analysis APIs. The `dotnet test` command automatically restores and builds before testing.

#### Semantic Kernel Build & Test Command

```bash
cd cloned_repos/semantic-kernel && \
git checkout dotnet-1.70.0 && \
cd dotnet && \
dotnet test SK-dotnet.slnx \
  --filter 'FullyQualifiedName!~SemanticKernel.IntegrationTests' \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
echo "Coverage data: cloned_repos/semantic-kernel/dotnet/TestResults/"
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
