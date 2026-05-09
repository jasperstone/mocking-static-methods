FROM mcr.microsoft.com/devcontainers/dotnet:1-10.0

# Fix the yarn repository GPG key issue before any apt operations
RUN sudo rm -f /etc/apt/sources.list.d/yarn.list || true

# Install clang and build tools required for .NET runtime native compilation
RUN apt-get update && apt-get install -y \
    clang-15 \
    llvm-15 \
    cmake \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Set clang-15 as the default
RUN update-alternatives --install /usr/bin/clang clang /usr/bin/clang-15 100 \
    && update-alternatives --install /usr/bin/clang++ clang++ /usr/bin/clang++-15 100
