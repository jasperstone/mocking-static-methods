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

The following table tracks the repositories cloned into the `cloned_repos/` directory with complete build and test scripts:

| Repository | GitHub Link | Build & Test Script |
|------------|-------------|---------------------|
| abp | [https://github.com/abpframework/abp](https://github.com/abpframework/abp) | `cd cloned_repos/abp && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/abp/TestResults/"` |
| aspnetcore | [https://github.com/dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | `cd cloned_repos/aspnetcore && git checkout main && ./restore.sh && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/aspnetcore/TestResults/"` |
| efcore | [https://github.com/dotnet/efcore](https://github.com/dotnet/efcore) | `cd cloned_repos/efcore && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/efcore/TestResults/"` |
| mono | [https://github.com/mono/mono](https://github.com/mono/mono) | `cd cloned_repos/mono && git checkout main && ./autogen.sh && make && make check && make coverage && echo "Coverage data available in cloned_repos/mono/coverage/"` |
| orleans | [https://github.com/dotnet/orleans](https://github.com/dotnet/orleans) | `cd cloned_repos/orleans && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/orleans/TestResults/"` |
| roslyn | [https://github.com/dotnet/roslyn](https://github.com/dotnet/roslyn) | `cd cloned_repos/roslyn && git checkout main && ./restore.sh && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/roslyn/TestResults/"` |
| runtime | [https://github.com/dotnet/runtime](https://github.com/dotnet/runtime) | `cd cloned_repos/runtime && git checkout main && ./build.sh && ./src/tests/build.sh && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/runtime/TestResults/"` |
| semantic-kernel | [https://github.com/microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) | `cd cloned_repos/semantic-kernel && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/semantic-kernel/TestResults/"` |
| server | [https://github.com/IdentityServer/IdentityServer4](https://github.com/IdentityServer/IdentityServer4) | `cd cloned_repos/server && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/server/TestResults/"` |
| subtitleedit | [https://github.com/SubtitleEdit/subtitleedit](https://github.com/SubtitleEdit/subtitleedit) | `cd cloned_repos/subtitleedit && git checkout main && dotnet restore && dotnet build && dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults && echo "Coverage data available in cloned_repos/subtitleedit/TestResults/"` |

> **Note:** Build commands may vary based on the specific repository structure and requirements. Some repositories may require additional setup steps or have custom build scripts. Always check the repository's README for specific build instructions.

### Coverage Report Generation

After running tests with coverage collection, you can generate HTML reports using:

```bash
# Install the reportgenerator tool (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html
```
