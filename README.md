# mocking-static-methods
Experiment in generating unit tests and mocks for code containing static method calls

## Setup

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
| aspnetcore | [https://github.com/dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | | |
| efcore | [https://github.com/dotnet/efcore](https://github.com/dotnet/efcore) | | |
| mono | [https://github.com/mono/mono](https://github.com/mono/mono) | | |
| orleans | [https://github.com/dotnet/orleans](https://github.com/dotnet/orleans) | | |
| roslyn | [https://github.com/dotnet/roslyn](https://github.com/dotnet/roslyn) | | |
| runtime | [https://github.com/dotnet/runtime](https://github.com/dotnet/runtime) | | q |
| semantic-kernel | [https://github.com/microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) | | |
| server | [https://github.com/IdentityServer/IdentityServer4](https://github.com/IdentityServer/IdentityServer4) | | |
| subtitleedit | [https://github.com/SubtitleEdit/subtitleedit](https://github.com/SubtitleEdit/subtitleedit) | | |

> **Note:** Build commands may vary based on the specific repository structure and requirements. Some repositories may require additional setup steps or have custom build scripts. Always check the repository's README for specific build instructions.

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

### Coverage Report Generation

After running tests with coverage collection, you can generate HTML reports using:

```bash
# Install the reportgenerator tool (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html
```
