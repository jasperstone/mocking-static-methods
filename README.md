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
