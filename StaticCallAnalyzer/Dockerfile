# StaticCallAnalyzer — containerized for reproducibility.
# Eliminates the host .NET 8 runtime requirement; only `docker` is needed.

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

# Copy project + restore first for layer caching.
COPY StaticCallAnalyzer.csproj ./
RUN dotnet restore StaticCallAnalyzer.csproj

# Copy sources and publish a self-contained-ish framework-dependent build.
COPY . ./
RUN dotnet publish StaticCallAnalyzer.csproj \
      -c Release \
      -o /out \
      --no-restore \
      /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /out/ /app/

# Quiet dotnet, keep output JSON-clean on stdout.
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Convention: caller mounts target source at /src (read-only).
ENTRYPOINT ["dotnet", "/app/StaticCallAnalyzer.dll"]
CMD ["/src"]
