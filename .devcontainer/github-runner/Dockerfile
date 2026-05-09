FROM mcr.microsoft.com/devcontainers/dotnet:1-10.0

# Fix yarn repository issue
RUN sudo rm -f /etc/apt/sources.list.d/yarn.list || true

# Install essential tools for GitHub Actions testing
RUN apt-get update && apt-get install -y \
    clang-15 \
    llvm-15 \
    cmake \
    build-essential \
    curl \
    git \
    jq \
    && rm -rf /var/lib/apt/lists/*

# Set clang-15 as default
RUN update-alternatives --install /usr/bin/clang clang /usr/bin/clang-15 100 \
    && update-alternatives --install /usr/bin/clang++ clang++ /usr/bin/clang++-15 100

# Install act (GitHub Actions local runner)
RUN curl -s https://raw.githubusercontent.com/nektos/act/master/install.sh | bash

# Create a directory for act cache
RUN mkdir -p /root/.cache/act
