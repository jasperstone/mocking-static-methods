FROM mcr.microsoft.com/devcontainers/dotnet:1-10.0

# Fix the yarn repository GPG key issue before any apt operations
RUN sudo rm -f /etc/apt/sources.list.d/yarn.list || true
