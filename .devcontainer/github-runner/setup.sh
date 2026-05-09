#!/bin/bash
set -e

echo "Setting up GitHub Actions local runner environment..."

# Install Python requirements
if [ -f "requirements.txt" ]; then
    pip install -r requirements.txt
fi

# Install dotnet tools
dotnet tool install -g dotnet-reportgenerator-globaltool || true

# Ensure act is executable
sudo chmod +x /usr/local/bin/act || true

echo ""
echo "✅ Setup complete!"
echo ""
echo "===================================================================="
echo "  GitHub Actions Local Runner - Quick Start"
echo "===================================================================="
echo ""
echo "To run a specific job from your workflow:"
echo "  act -j <job-name> --workflows .github/workflows/coverage-orchestrator.yml"
echo ""
echo "Examples:"
echo "  # Run just the ABP job"
echo "  act -j abp"
echo ""
echo "  # Run the aspnetcore job"
echo "  act -j aspnetcore"
echo ""
echo "  # List all jobs in the workflow"
echo "  act -l"
echo ""
echo "  # Dry run (see what would run without executing)"
echo "  act -n"
echo ""
echo "  # Run with workflow_dispatch event and inputs"
echo "  act workflow_dispatch -j abp --input repo=abp"
echo ""
echo "Advanced options:"
echo "  # Use a larger Docker image (more complete, slower)"
echo "  act -j abp -P ubuntu-latest=catthehacker/ubuntu:full-latest"
echo ""
echo "  # See verbose output"
echo "  act -j abp -v"
echo ""
echo "Documentation: https://github.com/nektos/act"
echo "===================================================================="
