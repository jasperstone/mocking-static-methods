# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Reproducible builds for 7 .NET OSS repos.

## Core Context

Build/infra agent. `.devcontainer/` exists. Each repo in `cloned_repos/` has its own SDK requirements.

## Learnings

- aspnetcore needs `git submodule update --init --recursive` and `source ./activate.sh`.
- aspnetcore tests need coverlet.collector added to ~137 test projects.
- efcore uses local SDK via `activate.sh` (10.0.102).
- Containerized build pattern: `docker run --rm -v "$(pwd)/cloned_repos/<repo>:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:10.0 bash -c "..."`.

## Recent Updates

- 2026-04-30 — Workflow audit findings: 6/7 jobs set `dotnet-version: 9.0.x` while their `global.json` requires 10.0.x (aspnetcore 10.0.101, efcore 10.0.102, orleans 10.0.102, roslyn 10.0.100-rc.2, sk 10.0.100). Runtime job has NO `setup-dotnet` step. EF Core sources `activate.sh` but doesn't export `DOTNET_ROOT` to `$GITHUB_ENV` like aspnetcore does — fragile. coverlet.collector is never added to test projects (README requires it for aspnetcore's 137 test projects).
